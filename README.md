# Keyboard Layout Extractor

A C# utility for extracting and exporting Windows keyboard layouts to multiple formats, including QMK-compatible header files. This tool helps keyboard firmware developers and enthusiasts to accurately replicate Windows keyboard layouts in their custom keyboard projects.

## Features

- 🔍 Extracts keyboard layouts directly from Windows Registry
- 🗺️ Maps virtual keys and character combinations
- ⚡ Identifies and exports dead key configurations
- 💾 Exports layouts in multiple formats:
  - JSON format for further processing
  - QMK-compatible header files for firmware development
- ⌨️ Supports all installed keyboard layouts
- 🔄 Handles shift states and special characters

## Requirements

- Windows operating system
- .NET Framework (compatible with the C# version you're using)
- Administrative privileges (for registry access)

## Installation

1. Clone the repository:
2. Open the solution in Visual Studio or your preferred C# IDE
3. Build the project


## Usage

1. Run the executable:
```bash
KeyboardLayoutExtractor.exe
```

2. The program will automatically:
   - Scan your system for installed keyboard layouts
   - Extract layout information
   - Generate files in the `exported_layouts` directory

### Output Files

The tool generates two types of files for each keyboard layout:

1. `{LayoutId}.json` - Contains complete layout data including:
   - Layout identification information
   - Virtual key mappings
   - Character mappings
   - Dead keys list

2. `keymap_{layoutid}.h` - QMK-compatible header file containing:
   - Layout definitions
   - Virtual key mappings
   - Dead key definitions

## Technical Details

### Key Components

- Uses Windows API (user32.dll) for keyboard layout interaction
- Registry access for layout enumeration
- Virtual key mapping and character code conversion
- Dead key detection and processing

### API Functions Used

- `GetKeyboardLayoutList`
- `LoadKeyboardLayout`
- `VkKeyScanEx`
- `MapVirtualKeyEx`
- `ToUnicodeEx`

## Contributing

Contributions are welcome! Please feel free to submit pull requests or create issues for bugs and feature requests.

### Development Guidelines

1. Follow existing code style and conventions
2. Add comments for complex operations
3. Update documentation for new features
4. Include appropriate error handling

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Windows API documentation
- QMK Firmware project for keyboard mapping standards

## Note on System Access

This program requires registry access to read keyboard layout information. Make sure to run it with appropriate permissions.

## Troubleshooting

### Common Issues

1. **Access Denied**
   - Run the program with administrative privileges
   - Check Windows security settings

2. **Missing Layouts**
   - Verify layouts are properly installed in Windows
   - Check registry permissions

### Error Reporting

When reporting issues, please include:
- Windows version
- Installed keyboard layouts
- Any error messages
- Generated output files

## Project Structure

```
KeyboardLayoutExtractor/
├── Program.cs              # Main program logic
├── exported_layouts/       # Generated output directory
│   ├── *.json             # Layout data in JSON format
│   └── keymap_*.h         # QMK-compatible header files
└── README.md              # This file
```
