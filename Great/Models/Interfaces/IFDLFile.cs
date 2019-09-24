namespace Great.Models.Interfaces
{
    public interface IFDLFile
    {
        string FileName { get; set; }
        string FilePath { get; }

        EFDLStatus EStatus { get; set; }
    }
}