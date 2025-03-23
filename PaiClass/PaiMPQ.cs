using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PaiClass
{
    public class PaiMPQ
    {
        public int HeaderOffset; // MPQ_HEADER_SIGNATURE Offset <-- should be located 0x200 * n.
        public uint HeaderSize; // Size of the Archive Header <-- usemap of war3 is fixed 0x20(32).
        public uint ArchiveSize; // Size of the MPQ Archive <-- MPQ Header + End of HashTable or Blocktable or Extended BlockTable.
        public ushort FormatVersion; // 0 = Format 1 (up to The Burning Crusade, warcraft3).
        public ushort SectorSizeShift; // In the archive, The size of each logical sector in the archive is 512 * 2^SectorSize.
        public int HashTableOffset; // HashTable's Start Offset, relative to MPQHeaderOffset.
        public int BlockTableOffset; // BlockTable's Start Offset, relative to MPQHeaderOffset.
        public uint HashTableEntries; // Number of entries in the HashTable. Must be a power of 2. Max Size is 2^19.
        public uint BlockTableEntries; // Number of entries in the BlockTable.  Max Size is 2^19.

        public void OpenMPQ(byte[] MPQFile)
        {
            for (int i = 0; i <= (MPQFile.Length / 512); i++)
            {
                if (BitConverter.ToUInt32(MPQFile, i * 0x200) == PaiConstant.MPQ_HEADER_SIGNATURE)
                {
                    HeaderOffset = 0x200 * i;
                    HeaderSize = 0x20;
                    ArchiveSize = BitConverter.ToUInt32(MPQFile, HeaderOffset + 0x8);
                    FormatVersion = BitConverter.ToUInt16(MPQFile, HeaderOffset + 0xC);
                    SectorSizeShift = BitConverter.ToUInt16(MPQFile, HeaderOffset + 0xE);
                    HashTableOffset = BitConverter.ToInt32(MPQFile, HeaderOffset + 0x10);
                    BlockTableOffset = BitConverter.ToInt32(MPQFile, HeaderOffset + 0x14);
                    HashTableEntries = BitConverter.ToUInt32(MPQFile, HeaderOffset + 0x18);
                    if (HashTableEntries > 0x8000) HashTableEntries = 0x8000;
                    BlockTableEntries = BitConverter.ToUInt32(MPQFile, HeaderOffset + 0x1C);
                    if (BlockTableEntries > 0x8000) BlockTableEntries = 0x8000;
                    break;
                }

            }
        }
        public void OpenMPQ(string filepath) => OpenMPQ(File.ReadAllBytes(filepath));

        public byte[] ExportHashTable(byte[] MPQFile)
        {
            byte[] hashtable = new byte[HashTableEntries * 0x10];
            int ht_offset = HashTableOffset + HeaderOffset;
            if (ht_offset < 0) ht_offset += HeaderOffset;
            if (ht_offset + HashTableEntries * 0x10 > MPQFile.Length)
                Array.Copy(MPQFile, ht_offset, hashtable, 0, MPQFile.Length - ht_offset);
            else
                Array.Copy(MPQFile, ht_offset, hashtable, 0, HashTableEntries * 0x10);

            return hashtable;
        }

        public byte[] ExportHashTable(string filepath) => ExportHashTable(File.ReadAllBytes(filepath));

        public byte[] ExportBlockTable(byte[] MPQFile)
        {
            byte[] blocktable = new byte[BlockTableEntries * 0x10];
            int bt_offset = BlockTableOffset + HeaderOffset;
            if (bt_offset < 0) bt_offset += HeaderOffset;
            if (bt_offset + BlockTableEntries * 0x10 > MPQFile.Length)
                Array.Copy(MPQFile, bt_offset, blocktable, 0, MPQFile.Length - bt_offset);
            else
                Array.Copy(MPQFile, bt_offset, blocktable, 0, BlockTableEntries * 0x10);

            return blocktable;
        }

        public byte[] ExportBlockTable(string filepath) => ExportBlockTable(File.ReadAllBytes(filepath));

        public byte[] Compact(byte[] MPQFile)
        {
            byte[] newMPQ = new byte[MPQFile.Length];
            byte[] hashtable = ExportHashTable(MPQFile);
            byte[] blocktable = ExportBlockTable(MPQFile);
            byte[] newHeader = new byte[32];

            Array.Copy(BitConverter.GetBytes(PaiConstant.MPQ_HEADER_SIGNATURE), 0, newMPQ, 0, 0x4);
            Array.Copy(BitConverter.GetBytes(HeaderSize), 0, newMPQ, 0x4, 0x4);
            Array.Copy(BitConverter.GetBytes(ArchiveSize), 0, newMPQ, 0x8, 0x4);
            Array.Copy(BitConverter.GetBytes(FormatVersion), 0, newMPQ, 0xC, 0x2);
            Array.Copy(BitConverter.GetBytes(SectorSizeShift), 0, newMPQ, 0xE, 0x2);

            return newMPQ;
        }

        public byte[] Compact(string filepath) => Compact(File.ReadAllBytes(filepath));
    }
}
