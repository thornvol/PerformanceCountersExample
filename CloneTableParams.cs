namespace TmsDataAccess.Sql
{
    public interface ICloneTableParams
    {
        string SourceTable { get; set; }
        string TargetTable { get; set; }
        string SourceConnStr { get; set; }
        string TargetConnStr { get; set; }
        string AsFileGroup { get; set; }
        string PrimaryKeys { get; set; }
        string AppendColumns { get; set; }
        bool ForceRebuild { get; set; }
        bool ForceDateTimeOverSmallDateTime { get; set; }
    }

    public sealed class CloneTableParams : ICloneTableParams
    {
        public string SourceTable { get; set; }
        public string TargetTable { get; set; }
        public string SourceConnStr { get; set; }
        public string TargetConnStr { get; set; }
        public string AsFileGroup { get; set; }
        public string PrimaryKeys { get; set; }
        public string AppendColumns { get; set; }
        public bool ForceRebuild { get; set; }
        public bool ForceDateTimeOverSmallDateTime { get; set; }

        public CloneTableParams()
        {
            this.AsFileGroup = "";
            this.PrimaryKeys = string.Empty;
            this.AppendColumns = string.Empty;
            this.ForceRebuild = false;
        }
    }
}