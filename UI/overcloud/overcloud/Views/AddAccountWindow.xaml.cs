﻿using System.Windows;
using System.Windows.Controls;
using DB.overcloud.Repository;
using DB.overcloud.Models;  // CloudAccountInfo 클래스 사용을 위해 추가
//using static overcloud.temp_class.TempClass;  // CloudAccountInfo 클래스 사용을 위해 추가
using OverCloud.Services;

namespace overcloud.Views
{
    public partial class AddAccountWindow : Window
    {
        private AccountService _accountService;     //수정 필요

        public AddAccountWindow(AccountService accountService)
        {

            InitializeComponent();
            _accountService = accountService;                                   //

        }

        private async void Confirm_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Confirm_Click call");
            string id = txtID.Text;
            string password = txtPassword.Password;
            string cloudType = (cloudComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            CloudStorageInfo accountInfo = new CloudStorageInfo
            {
                AccountId = id,
                AccountPassword = password,
                CloudType = cloudType,
				TotalCapacity = 0,
				UsedCapacity = 0,
                UserNum = 1
            };

            System.Diagnostics.Debug.WriteLine(cloudType);
            // ⭐ 객체 생성 없이 정적 메서드 직접 호출
            bool result = await _accountService.AddCloudStorage(accountInfo);
            System.Windows.MessageBox.Show(result ? "계정 추가 성공" : "계정 추가 실패");

            this.Close();
        }
    }
}
