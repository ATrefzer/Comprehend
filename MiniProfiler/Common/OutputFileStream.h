//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

#pragma once

#include "IOutputStream.h"


namespace CppEssentials
{

    /// Open mode for the file writer classes
    ///
    enum FileOpenMode
    {
        /// If the file exists it is removed and re-created
        CreateNew,

        /// If the file exists it is opened and the write pointer moved to the
        /// end of the file.
        Append
    };

    /// Write class for binary files
    class OutputFileStream : public IOutputStream
    {
    public:

        /// @param cacheSize    Size in bytes of internal cache. The cache is used when single bytes
        ///                     are written via #Write
        ///
        OutputFileStream(unsigned int cacheSize = 0);

        virtual ~OutputFileStream();

        /// Opens a file for byte wise access
        ///
        /// @param  filePath    Path to the file
        /// @param  eMode       Behavior if file already exists. (Delete or append)
        ///
        void Open(const wstring & filePath, FileOpenMode eMode = CreateNew);

        /// Closes the file. (Done automatically in destructor)
        ///
        void Close();

        // Interface IBinaryWriter
        virtual void Write(byte value);
        virtual void Write(const byte * data, UInt32 length);
        virtual void Flush();

    private:

        void _WriteChunk(const byte * data, UInt32 length);
        void Clear();

        /// Checks if file is open. If not a file_exception is thrown
        void VerifyFileOpen();

        /// Do not allow copy or assignment
        OutputFileStream(const OutputFileStream & writer);

        /// Do not allow copy or assignment
        OutputFileStream &operator==(const OutputFileStream & writer);

    private:

        /// Internal cache to increase performance if single bytes are written
        byte * _cache;

        /// Position in cache where the next byte is placed
        unsigned int _cacheIndex;

        /// Count of bytes currently in cache
        unsigned int _cachedBytes;

        /// Size of internal cache
        const unsigned int CACHE_SIZE;

        HANDLE _handle;

        void operator=(const OutputFileStream &);
    };

}