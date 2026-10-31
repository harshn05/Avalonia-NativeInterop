# Portable Avalonia UI + C++ Native Backend Application

This project is a blueprint for a **Cross-Platform Portable Desktop Application**. It seamlessly integrates **Avalonia UI (.NET 10)** with a **C++ Native Shared Library (DLL)** using **P/Invoke (Platform Invoke)** infrastructure. 

The standout feature of this setup is its **100% Out-of-Source Automated Build Pipeline**. It enforces strict environment isolation, ensuring the compiled application runs smoothly in fresh, isolated environments like **Windows Sandbox** without requiring external runtime dependencies (`libgcc`, `winpthread`, etc.), while keeping your source workspace entirely pristine.

---

## 📁 Project Architecture & Workspace Layout

Your clean project root folder contains only these four core files:
1. `CMakeLists.txt` - Orchestrates the entire C++ compilation and .NET automated publish pipeline.
2. `Calculator.cpp` - Implements the native C++ functionality (Safe dynamic addition and Double-precision Factorial logic).
3. `PortableAvaloniaApp.csproj` - Standardizes the optimized Single-File Avalonia .NET application settings.
4. `Program.cs` - Handles the UI window lifecycle, state parsing, and native P/Invoke method mappings.

---

## 🛠️ Code Deep Dive & Substantive Explanations

### 1. Native Backend (`Calculator.cpp`)
* **`extern "C"`**: Suppresses C++ name mangling. This ensures the C# P/Invoke layer can resolve the exact exported function identifiers at runtime.
* **`__declspec(dllexport)`**: Instructs the Windows operating system to expose these native entry points outside the compiled DLL binary.
* **`double CalculateFactorial`**: Employs a 64-bit floating-point `double` rather than a standard integer. This gracefully bypasses data truncation or bit wrap-around (negative overflows), representing massive numbers like $100!$ cleanly in scientific notation (`9.3326E+157`).

### 2. UI & Interop Bridge (`Program.cs`)
* **`[DllImport("NativeCalc.dll")]`**: Directs the CLR to dynamically bind and resolve native dependencies out of the active runtime execution directory.
* **`TextBox.Watermark`**: Used for UI text prompts. In modern Avalonia configurations, this can be seamlessly swapped with `PlaceholderText` to comply with the latest framework APIs.
* **Robust Exception Layer**: Traps runtime boundary exceptions such as `DllNotFoundException` to gracefully inform the user via the UI if the sandbox blocks file access.

### 3. Build & Deployment Optimization (`PortableAvaloniaApp.csproj`)
* **`<GenerateAssemblyInfo>false</GenerateAssemblyInfo>`**: A vital optimization. This explicitly halts parallel automatic build metadata generation during concurrent tasks, completely mitigating the infamous duplicate attribute compiler error (`CS0579`) inside Ninja/MinGW environments.
* **`<BuiltInComInteropSupport>false</BuiltInComInteropSupport>`**: Disables standard Windows COM mapping interfaces, stripping out unnecessary metadata layers to streamline the assembly tree for strict single-file trimming.

### 4. Automated Build Pipeline (`CMakeLists.txt`)
The build pipeline operates in three distinct phases:
1. **Static Linker Enforcement**: The `if(MINGW)` branch injects `-static -static-libgcc -static-libstdc++ -Wl,-Bstatic,--whole-archive -lwinpthread` instructions. This embeds raw GCC/pthread runtime assets directly into the dynamic library wrapper, eliminating missing dependency crashes in fresh environments.
2. **`publish_dotnet` Custom Target**: Triggers compilation of the native target and temporarily mirrors the resulting DLL into the staging path, allowing the `.NET CLI` to securely bundle it into a unified executable via `-p:PublishSingleFile=true`.
3. **Post-Build Auto-Sweeper**: Immediately after a successful publish command, CMake sweeps through the workspace directory, scrubbing away temporary `bin/`, `obj/`, and intermediate `.dll` traces to maintain absolute source branch purity.

---

## 🚀 Building & Deploying the Project

### Prerequisites
* Windows 11 / Windows Sandbox
* **.NET 10.0 SDK** (or your specified runtime framework)
* **CMake 3.16 or higher**
* Compiler Toolchain: **MinGW64 (via MSYS2)** or **MSVC (Visual Studio 2022/2026)**

### Compilation Instructions

#### Option A: Using MinGW (Ninja Compiler) - Recommended for Sandbox testing
Open your terminal inside the project root folder and execute the following commands:

```bash
# 1. Generate out-of-source build configuration trees
cmake -S . -B build_mingw -G "Ninja"

# 2. Compile native source and trigger the automated single-file publish target
cmake --build build_mingw --target publish_dotnet
```

#### Option B: Using MSVC (Visual Studio Linker)
```bash
# 1. Generate Visual Studio project trees
cmake -S . -B build_msvc

# 2. Compile and execute the bundled build deployment
cmake --build build_msvc --target publish_dotnet
```

---

## 🎯 Final Isolated Production Build Location

Once the automation target reports a successful compilation, your self-contained deployment package will be waiting inside:
👉 **`[Your-Project-Root]/build_mingw/dotnet_publish/`** (or `build_msvc/dotnet_publish/`)

You can copy this standalone folder directly into **Windows Sandbox** or any bare Windows machine. The application will launch instantly and execute its C++ calculations perfectly, completely independent of external dependencies!
