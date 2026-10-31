cmake_minimum_required(VERSION 3.16)
project(PortableAvaloniaApp LANGUAGES CXX)

# --- 1. Global Policies & Safeguards ---
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Enforce out-of-source builds to prevent source directory pollution
if(CMAKE_CURRENT_SOURCE_DIR STREQUAL CMAKE_CURRENT_BINARY_DIR)
    message(FATAL_ERROR "Error: In-source builds are strictly prohibited! Please create a separate build directory (e.g., build_mingw) and run CMake from there.")
endif()

# --- 2. Toolchain Optimization (MinGW Static Isolation) ---
if(MINGW)
    message(STATUS "Configuring isolated standalone runtime environment flags for MinGW...")
    set(STATIC_LINK_FLAGS "-static -static-libgcc -static-libstdc++ -Wl,-Bstatic,--whole-archive -lwinpthread -Wl,--no-whole-archive")
    set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} ${STATIC_LINK_FLAGS}")
    set(CMAKE_MODULE_LINKER_FLAGS "${CMAKE_MODULE_LINKER_FLAGS} ${STATIC_LINK_FLAGS}")
endif()

# --- 3. Native Target Architecture ---
add_library(NativeCalc SHARED Calculator.cpp)

# Apply Windows dynamic link library definitions and export configurations
target_compile_definitions(NativeCalc PRIVATE -D_USRDLL -D_WINDLL)
set_target_properties(NativeCalc PROPERTIES 
    PREFIX ""
    IMPORT_PREFIX ""
)

# --- 4. .NET Integration Pipeline Engine ---
find_program(DOTNET_EXE NAMES dotnet REQUIRED)

set(DOTNET_PROJECT_DIR ${CMAKE_CURRENT_SOURCE_DIR})
set(PUBLISH_OUTPUT_DIR ${CMAKE_CURRENT_BINARY_DIR}/dotnet_publish)

# System architecture paths declaration for sweeping routines
file(TO_NATIVE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/NativeCalc.dll" SOURCE_DLL_PATH)

# Programmatic out-of-source build execution and asset packaging pipeline
add_custom_target(publish_dotnet
    # Step A: Stage the compiled native DLL into the source root for .NET deployment binding
    COMMAND ${CMAKE_COMMAND} -E copy $<TARGET_FILE:NativeCalc> "${CMAKE_CURRENT_SOURCE_DIR}/"
    
    # Step B: Invoke the .NET toolchain to restore packages and publish a trimmed single-file binary
    COMMAND ${DOTNET_EXE} restore
    COMMAND ${DOTNET_EXE} publish -c Release -r win-x64 
            --self-contained true 
            -p:PublishSingleFile=true 
            -p:PublishTrimmed=true 
            -p:DebugType=None 
            -o "${PUBLISH_OUTPUT_DIR}"
            
    # Step C: Execute comprehensive workspace cleanup to maintain source repository purity
    COMMAND ${CMAKE_COMMAND} -E remove -f "${SOURCE_DLL_PATH}"
    COMMAND ${CMAKE_COMMAND} -E remove_directory "${CMAKE_CURRENT_SOURCE_DIR}/bin"
    COMMAND ${CMAKE_COMMAND} -E remove_directory "${CMAKE_CURRENT_SOURCE_DIR}/obj"
    COMMAND ${CMAKE_COMMAND} -E remove_directory "${CMAKE_CURRENT_SOURCE_DIR}/dotnet_obj"
    COMMAND ${CMAKE_COMMAND} -E remove_directory "${CMAKE_CURRENT_SOURCE_DIR}/dotnet_bin"
    
    WORKING_DIRECTORY "${DOTNET_PROJECT_DIR}"
    VERBATIM
    COMMENT "Executing production binary layout optimizations and cleaning up staging workspace..."
)

# Ensure the native shared library compiles successfully before launching the .NET assembly publisher
add_dependencies(publish_dotnet NativeCalc)
