namespace LocalOJ
{
    public class TestData
    {
        public string Input { get; set; } = "";
        public string ExpectedOutput { get; set; } = "";
        public string ActualOutput { get; set; } = "";
        public StatusCodes StatusCode { get; set; } = StatusCodes.UnTested;
    }
    public enum StatusCodes
    {
        UnTested,
        Right,
        Wrong,
        TimeOut
    }
}
