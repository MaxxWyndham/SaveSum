using System;
using System.IO;

namespace SaveSum
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), "SAVE*."))
            {
                uint checksum = 0;
                uint checksum2 = 0;

                using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(file)))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    ms.Seek(0x28c, SeekOrigin.Begin);

                    // 0x0  => 0xf   save name
                    // 0x10 => 0x1f  car name
                    // 0x20 => 0x2d  player 1 name
                    // 0x2e => 0x3b  player 2 name

                    // 100x uint32   race info
                    // 48x uint32    opponent owned

                    flipEndian(ms);  // credits
                    flipEndian(ms);  // rank
                    flipEndian(ms);  // skill level
                    flipEndian(ms);  // game completed
                    flipEndian(ms);  // number of cars
                    for (int j = 0; j < 60; j++) { flipEndian(ms); } // cars available
                    flipEndian(ms);  // current car index
                    flipEndian(ms);  // current race index
                    flipEndian(ms);  // redo race index
                    flipEndian(ms);  // max or die
                    for (int j = 0; j < 3; j++) { flipEndian(ms); } // a p o
                    flipEndian(ms);  // version
                    // checksum

                    ms.Seek(0, SeekOrigin.Begin);

                    while (br.BaseStream.Position < br.BaseStream.Length - 4)
                    {
                        checksum2 = (uint)(br.ReadByte() ^ 0xbd) + checksum;
                        checksum ^= checksum2 << 25;
                        checksum ^= checksum2 >> 7;
                    }
                }

                using (BinaryWriter bw = new BinaryWriter(File.Open(file, FileMode.Open)))
                {
                    bw.BaseStream.Seek(-4, SeekOrigin.End);
                    bw.Write(checksum);
                }
            }
        }

        private static void flipEndian(MemoryStream ms)
        {
            byte[] swap = new byte[4];

            ms.Read(swap, 0, 4);

            Array.Reverse(swap);

            ms.Seek(-4, SeekOrigin.Current);
            ms.Write(swap, 0, 4);
        }
    }
}
