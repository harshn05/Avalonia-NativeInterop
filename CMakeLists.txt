cmake_minimum_required(VERSION 3.16)
project(PortableAvaloniaApp LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

if(MINGW)
    message(STATUS "Configuring static linking for MinGW runtime libraries...")
    set(STATIC_LINK_FLAGS "-static -static-libgcc -static-libstdc++ -Wl,-Bstatic,--whole-archive -lwinpthread -Wl,--no-whole-archive")
    set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} ${STATIC_LINK_FLAGS}")
    set(CMAKE_MODULE_LINKER_FLAGS "${CMAKE_MODULE_LINKER_FLAGS} ${STATIC_LINK_FLAGS}")
endif()

# Define the C++ Native Shared Library (Strictly binary folder ke andar hi banegi)
add_library(NativeCalc SHARED Calculator.cpp)
target_compile_definitions(NativeCalc PRIVATE -D_USRDLL -D_WINDLL)

set_target_properties(NativeCalc PROPERTIES 
    PREFIX ""
    IMPORT_PREFIX ""
)

find_program(DOTNET_EXE NAMES dotnet REQUIRED)

set(DOTNET_PROJECT_DIR ${CMAKE_CURRENT_SOURCE_DIR})
set(PUBLISH_OUTPUT_DIR ${CMAKE_CURRENT_BINARY_DIR}/dotnet_publish)

# Native paths resolution for strict sweeping operation
file(TO_NATIVE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/NativeCalc.dll" SOURCE_DLL_PATH)

add_custom_target(publish_dotnet
    # 1. Native C++ DLL ko build folder se temporary source root me copy karna taaki .NET framework inject kar sake
    COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_BINARY_DIR}/NativeCalc.dll ${CMAKE_CURRENT_SOURCE_DIR}/NativeCalc.dll
    
    # 2. Stable internal restore aur executable optimization pipeline publish execution
    COMMAND ${DOTNET_EXE} restore
    COMMAND ${DOTNET_EXE} publish -c Release -r win-x64 
            --self-contained true 
            -p:PublishSingleFile=true 
            -p:PublishTrimmed=true 
            -p:DebugType=None 
            -o ${PUBLISH_OUTPUT_DIR}
            
    # 3. Post-Build Sweeper: Source workspace ko 100% spotless aur pollution-free karna
    COMMAND ${CMAKE_COMMAND} -E remove -f ${SOURCE_DLL_PATH}
    COMMAND ${CMAKE_COMMAND} -E remove_directory ${CMAKE_CURRENT_SOURCE_DIR}/bin
    COMMAND ${CMAKE_COMMAND} -E remove_directory ${CMAKE_CURRENT_SOURCE_DIR}/obj
    COMMAND ${CMAKE_COMMAND} -E remove_directory ${CMAKE_CURRENT_SOURCE_DIR}/dotnet_obj
    COMMAND ${CMAKE_COMMAND} -E remove_directory ${CMAKE_CURRENT_SOURCE_DIR}/dotnet_bin
    
    WORKING_DIRECTORY ${DOTNET_PROJECT_DIR}
    COMMENT "Executing safe standalone bundle and sweeping temporary files from root directory..."
)

add_dependencies(publish_dotnet NativeCalc)
