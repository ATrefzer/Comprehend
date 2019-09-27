//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

#pragma once
#include "DataTypes.h"
namespace CppEssentials
{
    /// Defines random access contract for binary data.
    class IRandomAccess
    {
    public:
        virtual ~IRandomAccess() {}

        ///
        /// Reads binary data
        /// Throws an exception if the index is out of range. (>= #GetLength)
        /// Note that this works differently as in #IOutputStream
        ///
        /// @param data         Pointer to allocated byte array
        /// @param length       Number of bytes to read into the byte array
        /// @param index        Index where to start reading
        ///
        /// @return UInt32      Actual read bytes. (If not all bytes are available)
        ///
        virtual UInt32 Read(byte * data, UInt32 length, UInt32 index) = 0;

        ///
        /// Writes binary data
        /// If there is not enough space or the index is out of range the target is reallocated
        ///
        /// @param data         Pointer to byte array
        /// @param length       Number of bytes to write
        /// @param index        Index where to start writing
        ///
        /// @return UInt32      Actual written bytes. (Always length)
        ///
        virtual void Write(const byte * data, UInt32 length, UInt32 index) = 0;

        /// Returns the number of available bytes
        ///
        virtual UInt32 GetLength() const = 0;

    };
}