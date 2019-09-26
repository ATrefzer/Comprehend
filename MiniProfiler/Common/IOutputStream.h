//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

#pragma once
#include "DataTypes.h"
namespace CppEssentials
{
    /// Interface for a general sequential binary writer
    class IOutputStream
    {
    public:
        virtual ~IOutputStream() {};

        /// Writes a single byte to the end of the stream
        /// This method is buffered. To ensure that the bytes are written to the device
        /// call #Flush. The data target resizes automatically if necessary.
        ///
        virtual void Write(byte value) = 0;

        /// Writes a byte array to the end of the stream
        /// This method is not buffered. If there are still bytes in the buffer
        /// they are flushed first. The data target resizes automatically if necessary.
        ///
        /// @param data     Bytes to write
        /// @param length   Size of buffer in bytes
        ///
        virtual void Write(const byte * data, UInt32 length) = 0;

        virtual void Flush() = 0;
    };
}