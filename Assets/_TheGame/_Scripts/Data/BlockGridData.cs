namespace _TheGame._Scripts.Data
{
    [System.Serializable]
    public class BlockGridData
    {
        public int id;   // Block id'si (moveData içindeki id ile eşleşebilir)
        public int col;  // Grid kolon (0'dan GameData.BoardSize-1'e kadar)
        public int row;  // Grid satır (0'dan GameData.BoardSize-1'e kadar)
    }
}