//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

#include "BinaryTools.h"

namespace CppEssentials
{
    BinaryTools::BinaryTools(void)
    {
    }

    BinaryTools::~BinaryTools(void)
    {
    }

    void BinaryTools::SwapBuffer(byte * buffer, UInt32 length)
    {
        if (length == 0 || length == 1)
        {
            return;
        }

        byte temp = 0;
        byte * end = buffer + length - 1;

        while (buffer < end)
        {
            temp = *buffer;
            *buffer = *end;
            *end = temp;
            buffer++;
            end--;
        }
    }

}