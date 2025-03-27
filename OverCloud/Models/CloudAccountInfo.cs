using System;
using Org.BouncyCastle.Asn1.Tsp;

//계정 정보 클래스
public class CloudAccountInfo
{   
    public int UserNum {  get; set; } //PK
    public string ID { get; set; }
    public string Password { get; set; }
    public string CloudType { get; set; }

    // 실시간으로 가져올 용량 정보
    public long TotalSize { get; set; }
    public long UsedSize { get; set; }
}