﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Picofy.TorshifyHelper
{
    public class Constants
    {
        internal static readonly byte[] ApplicationKey = new Byte[]
        {
            0x01, 0x56, 0xAE, 0xED, 0x54, 0xA9, 0x3B, 0x7F, 0x9E, 0xAB, 0x35, 0x54, 0x36, 0x48, 0x95, 0x49,
            0xF0, 0xA1, 0x23, 0x5D, 0x15, 0x7F, 0x05, 0xE4, 0x11, 0xC2, 0x94, 0x4D, 0xAE, 0xCA, 0xE7, 0x24,
            0x07, 0xED, 0xB0, 0x51, 0xA8, 0xF6, 0xF3, 0x22, 0xC7, 0x00, 0x9B, 0x07, 0x97, 0xF7, 0xBA, 0x38,
            0x30, 0x37, 0xE8, 0x60, 0x34, 0xAF, 0x42, 0xC7, 0xFE, 0x25, 0x2F, 0xA2, 0x22, 0x8C, 0xE3, 0xE6,
            0x34, 0x71, 0xC2, 0xCF, 0x99, 0x90, 0x5C, 0x75, 0xB4, 0xDB, 0xB4, 0x34, 0x3E, 0x21, 0xBB, 0xD8,
            0x09, 0xD8, 0x42, 0x2B, 0xED, 0x49, 0x97, 0x19, 0x4C, 0x87, 0x8D, 0x44, 0x9E, 0x19, 0x4F, 0x73,
            0xFA, 0xDF, 0x6A, 0xA4, 0xFE, 0xDA, 0xBD, 0xFD, 0xA1, 0xF1, 0x8B, 0x41, 0xE3, 0x5C, 0x3A, 0x5D,
            0x9E, 0xFD, 0xD7, 0xA5, 0x4D, 0x6A, 0x29, 0x0A, 0x0E, 0x58, 0xC5, 0xB6, 0xA9, 0xE0, 0x89, 0x7C,
            0xEA, 0x00, 0xE1, 0x34, 0x6F, 0x84, 0x30, 0xC8, 0xC4, 0x2B, 0xF3, 0x00, 0xC0, 0x34, 0x9E, 0x99,
            0x27, 0x8E, 0xD0, 0xCA, 0x1F, 0x19, 0x5F, 0xA0, 0x5A, 0x61, 0x63, 0x6F, 0x40, 0x2F, 0x7F, 0x4B,
            0xE3, 0x61, 0x58, 0xBF, 0x8A, 0x79, 0x3F, 0x2E, 0x09, 0xD9, 0x12, 0x5C, 0x1E, 0xEE, 0xED, 0x59,
            0xDF, 0x69, 0xA9, 0x61, 0xA6, 0xE8, 0xEB, 0x0A, 0x04, 0x68, 0x59, 0xF7, 0xD1, 0xFF, 0x40, 0xEC,
            0x72, 0xA8, 0x43, 0xAC, 0x4C, 0xF1, 0x2E, 0xFC, 0x76, 0x45, 0xF8, 0x21, 0x12, 0xE4, 0x34, 0x71,
            0x2B, 0x4F, 0x8C, 0x8D, 0x39, 0xCA, 0x5E, 0x88, 0xFF, 0x7D, 0x0E, 0x76, 0x99, 0xB1, 0xCD, 0x76,
            0x80, 0x8A, 0x69, 0xED, 0xE6, 0x0A, 0x52, 0x79, 0xF4, 0x2C, 0xB9, 0xB7, 0x25, 0xF6, 0x66, 0xAD,
            0xD3, 0xE6, 0x2A, 0x61, 0xF5, 0x61, 0x31, 0xE8, 0xE1, 0x32, 0xAD, 0xE7, 0x93, 0x68, 0x77, 0xD3,
            0x01, 0xA2, 0xF7, 0x6E, 0x4E, 0x7E, 0x85, 0x58, 0x14, 0x65, 0xBF, 0xF1, 0x92, 0xC9, 0x39, 0x3F,
            0x93, 0xB9, 0x6C, 0xB8, 0x66, 0x9B, 0x06, 0x0C, 0x7E, 0x05, 0x0B, 0x65, 0xF5, 0x5A, 0x4F, 0x38,
            0x29, 0x12, 0xD8, 0x00, 0xE5, 0xB7, 0x4C, 0xB3, 0xDC, 0x07, 0xE8, 0x4B, 0xEE, 0x78, 0x09, 0xAA,
            0x87, 0x38, 0x26, 0x03, 0x7D, 0x6C, 0x66, 0xE2, 0x4F, 0x03, 0x0B, 0x0F, 0xC2, 0xE2, 0xF2, 0x4D,
            0x38
        };

        internal const string UserAgent = "picofy";
        internal static readonly string CacheFolder = Path.Combine(Directory.GetCurrentDirectory(), "PicofyData", "Cache");
        internal static readonly string SettingsFolder = Path.Combine(Directory.GetCurrentDirectory(), "PicofyData", "Settings");
    }
}