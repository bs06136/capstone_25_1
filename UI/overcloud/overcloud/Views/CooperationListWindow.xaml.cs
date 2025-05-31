using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DB.overcloud.Models;
using OverCloud.Services;

namespace overcloud.Views
{
    public partial class CooperationListWindow : Window
    {
        public ObservableCollection<CooperationItem> CooperationItems { get; set; }
        private LoginController controller;
        private string user_id;

        public CooperationListWindow(LoginController controller, string user_id)
        {
            InitializeComponent();
            this.controller = controller;
            this.user_id = user_id;

            // 협업 리스트 초기화 (예: DB에서 coop_id 목록 가져오기)
            CooperationItems = new ObservableCollection<CooperationItem>();

            List<string> coopIds = controller.CoopUserRepository.connected_cooperation_account_nums(user_id);
            foreach (var coopId in coopIds)
            {
                CooperationItems.Add(new CooperationItem
                {
                    CooperationName = coopId, // 협업 이름 대신 coopId 사용
                    CoopId = coopId,
                 
                });
            }

            CooperationListBox.ItemsSource = CooperationItems;
        }

        private void LoadCoopAccounts(string coop_id)
        {
            Debug.WriteLine("협업 계정 불러오기 시작");

            // coop_id에 속한 참여자 ID 가져오기
            List<string> userIds = controller.CoopUserRepository.GetUsersByCoopId(coop_id);
           

            // 계정 리스트를 ListBox에 바인딩
            AccountListBox.ItemsSource = userIds;

            Debug.WriteLine("협업 계정 목록 출력 완료");
        }

        private void CooperationListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = CooperationListBox.SelectedItem as CooperationItem;
            if (selected != null)
            {
                // coop_id를 이용해서 참여 계정 로드
                LoadCoopAccounts(selected.CoopId);
            }
        }

        public class CooperationItem
        {
            public string CooperationName { get; set; }
            public string CoopId { get; set; } // coop_id 추가
        }
    }
}
