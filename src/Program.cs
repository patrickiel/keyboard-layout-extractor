using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace KeyboardLayoutExtractor
{
    class Program
    {
        #region Win32 API
        [DllImport("user32.dll")]
        private static extern int GetKeyboardLayoutList(int nBuff, [Out] IntPtr[] lpList);

        [DllImport("user32.dll")]
        private static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

        [DllImport("user32.dll")]
        private static extern short VkKeyScanEx(char ch, IntPtr dwhkl);

        [DllImport("user32.dll")]
        private static extern int MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);

        [DllImport("user32.dll")]
        private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff,
            uint wFlags, IntPtr dwhkl);

        private const int KLF_NOTELLSHELL = 0x00000080;
        private const int MAPVK_VK_TO_CHAR = 2;
        #endregion

        public class KeyboardLayout
        {
            public string LayoutId { get; set; }
            public string LayoutName { get; set; }
            public Dictionary<int, string> VirtualKeyMap { get; set; } = new Dictionary<int, string>();
            public Dictionary<char, string> CharacterMap { get; set; } = new Dictionary<char, string>();
            public List<string> DeadKeys { get; set; } = new List<string>();
        }

        static void Main(string[] args)
        {
            var layouts = GetAllKeyboardLayouts();
            ExportLayouts(layouts);
        }

        static List<KeyboardLayout> GetAllKeyboardLayouts()
        {
            var layouts = new List<KeyboardLayout>();

            // Get all keyboard layouts from registry
            using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Keyboard Layouts"))
            {
                if (key == null) return layouts;

                foreach (var layoutId in key.GetSubKeyNames())
                {
                    using (var layoutKey = key.OpenSubKey(layoutId))
                    {
                        if (layoutKey == null) continue;

                        var layout = new KeyboardLayout
                        {
                            LayoutId = layoutId,
                            LayoutName = (string)layoutKey.GetValue("Layout Text", "Unknown")
                        };

                        // Load the keyboard layout
                        var hkl = LoadKeyboardLayout(layoutId, KLF_NOTELLSHELL);
                        if (hkl != IntPtr.Zero)
                        {
                            // Map virtual keys
                            MapVirtualKeys(layout, hkl);

                            // Map characters
                            MapCharacters(layout, hkl);

                            // Find dead keys
                            FindDeadKeys(layout, hkl);

                            layouts.Add(layout);
                        }
                    }
                }
            }

            return layouts;
        }

        static void MapVirtualKeys(KeyboardLayout layout, IntPtr hkl)
        {
            // Map standard virtual keys (A-Z, 0-9, etc.)
            for (int vk = 0x20; vk < 0x7F; vk++)
            {
                var result = MapVirtualKeyEx((uint)vk, MAPVK_VK_TO_CHAR, hkl);
                if (result != 0)
                {
                    layout.VirtualKeyMap[vk] = ((char)result).ToString();
                }
            }
        }

        static void MapCharacters(KeyboardLayout layout, IntPtr hkl)
        {
            // Map printable ASCII characters
            for (char c = ' '; c <= '~'; c++)
            {
                var vk = VkKeyScanEx(c, hkl);
                if (vk != -1)
                {
                    var virtualKey = vk & 0xFF;
                    var shift = (vk >> 8) & 0xFF;
                    layout.CharacterMap[c] = $"VK_{virtualKey:X2}" + (shift != 0 ? $" + SHIFT" : "");
                }
            }
        }

        static void FindDeadKeys(KeyboardLayout layout, IntPtr hkl)
        {
            // Test for common dead keys
            byte[] keyState = new byte[256];
            StringBuilder buff = new StringBuilder(5);

            for (uint vk = 0; vk < 256; vk++)
            {
                var result = ToUnicodeEx(vk, 0, keyState, buff, buff.Capacity, 0, hkl);
                if (result < 0) // Negative result indicates a dead key
                {
                    layout.DeadKeys.Add($"VK_{vk:X2}");
                }
            }
        }

        static void ExportLayouts(List<KeyboardLayout> layouts)
        {
            // Create output directory
            Directory.CreateDirectory("exported_layouts");

            foreach (var layout in layouts)
            {
                // Export to JSON
                File.WriteAllText(
                    Path.Combine("exported_layouts", $"{layout.LayoutId}.json"),
                    System.Text.Json.JsonSerializer.Serialize(layout, new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                    })
                );

                // Export to QMK format
                ExportToQMK(layout);
            }
        }

        static void ExportToQMK(KeyboardLayout layout)
        {
            var qmkPath = Path.Combine("exported_layouts", $"keymap_{layout.LayoutId.ToLower()}.h");
            using (var writer = new StreamWriter(qmkPath))
            {
                // Write header
                writer.WriteLine("/* Generated from Windows keyboard layout");
                writer.WriteLine($" * Layout: {layout.LayoutName}");
                writer.WriteLine($" * Layout ID: {layout.LayoutId}");
                writer.WriteLine(" */");
                writer.WriteLine();
                writer.WriteLine("#pragma once");
                writer.WriteLine();
                writer.WriteLine("#include \"keymap.h\"");
                writer.WriteLine();

                // Write layout name
                var layoutName = layout.LayoutId.Replace("KBD", "").ToUpper();
                writer.WriteLine($"// {layout.LayoutName} ({layoutName}) keyboard layout definitions");
                writer.WriteLine();

                // Write virtual key mappings
                foreach (var kvp in layout.VirtualKeyMap)
                {
                    writer.WriteLine($"#define {layoutName}_{kvp.Value} KC_#{kvp.Key:X2}");
                }

                writer.WriteLine();

                // Write dead keys
                if (layout.DeadKeys.Any())
                {
                    writer.WriteLine("// Dead keys");
                    foreach (var deadKey in layout.DeadKeys)
                    {
                        writer.WriteLine($"#define {layoutName}_DEAD_{deadKey} {deadKey}");
                    }
                }
            }
        }
    }
}