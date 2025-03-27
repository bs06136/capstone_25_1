using OverCloud.Models;
using OverCloud.Services;
using System;
using System.Collections.Generic;

class Program
{
    static IAccountService service = new AccountService();

    static void Main()
    {
        while (true)
        {
            Console.WriteLine("\n--- 메뉴 ---");
            Console.WriteLine("1. 전체 계정 조회");
            Console.WriteLine("2. 계정 추가");
            Console.WriteLine("3. 계정 삭제");
            Console.WriteLine("0. 종료");
            Console.Write("선택: ");

            switch (Console.ReadLine())
            {
                case "1": ViewAccounts(); break;
                case "2": AddAccount(); break;
                case "3": DeleteAccount(); break;
                case "0": return;
                default: Console.WriteLine("잘못된 입력"); break;
            }
        }
    }

    static void ViewAccounts()
    {
        var list = service.GetAllAccounts();
        Console.WriteLine("\n=== 계정 목록 ===");
        foreach (var acc in list)
            Console.WriteLine($"[{acc.UserNum}] {acc.ID} / {acc.Password} / {acc.CloudType}");
    }

    static void AddAccount()
    {
        Console.Write("ID: ");
        string id = Console.ReadLine();
        Console.Write("Password: ");
        string pw = Console.ReadLine();
        Console.Write("CloudType: ");
        string ct = Console.ReadLine();

        var account = new CloudAccountInfo { ID = id, Password = pw, CloudType = ct };
        Console.WriteLine(service.InsertAccount(account) ? "저장 완료" : "저장 실패");
    }

    static void DeleteAccount()
    {
        Console.Write("삭제할 user_num 입력: ");
        if (int.TryParse(Console.ReadLine(), out int num))
            Console.WriteLine(service.DeleteAccountByUserNum(num) ? "삭제 완료" : "삭제 실패");
        else
            Console.WriteLine("숫자를 입력해주세요.");
    }
}
