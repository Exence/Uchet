using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Uchet.Classes;
using Microsoft.Office.Interop.Word;
using Application = Microsoft.Office.Interop.Word.Application;
using Uchet.Views;

namespace Uchet
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Flags.isStarted = false; ///Проверка запуска учета
                                     ///Создаем таймер
                                     ///
            DispatcherTimer LiveTime = new DispatcherTimer();
            LiveTime.Interval = TimeSpan.FromSeconds(1);
            LiveTime.Tick += timer_Tick;
            LiveTime.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            LableTime.Content = DateTime.Now.ToString("HH:mm:ss"); ///Текущее время
            if (Flags.isStarted)
            {
                TimeSpan SignalTime = DateTime.Now - Flags.ConvertedTime;
                LableSignalTime.Content = SignalTime.ToString(@"hh\:mm\:ss");
            }
        }

        private void SummLabelInt(Label finalLabel, Label intLabel, int number)
        {
            number += Convert.ToInt16(intLabel.Content.ToString());
            finalLabel.Content = number.ToString();
        }

        private void FindPercent(Label finalLabel, Label intLabelArrive, Label intLabelShoulCome)
        {
            double percent = Math.Round(Convert.ToDouble(intLabelArrive.Content.ToString()) / Convert.ToDouble(intLabelShoulCome.Content.ToString()) * 100, 2);
            finalLabel.Content = percent.ToString() + "%";
        }

        /*private void SubLabelInt(Label finalLabel, Label intLabel, int number)
        {
            number = Convert.ToInt16(intLabel.Content.ToString()) - number;
            finalLabel.Content = number.ToString();
        }*/

        private void ButtonClose_Click(object sender, RoutedEventArgs e) => Close();

        private void RefreshGridUsers()
        {
            ///Формирование основной таблицы прибытия

            BindingList<ArriveUser> arriveUsers = new BindingList<ArriveUser>();
            Rank rank = null;
            User usr = null;
            int selectedIndex = GridUsers.SelectedIndex;
            int num, onService = 0, absent = 0, ch10 = 0, ch15 = 0, ch20 = 0, noArrived = 0;
            string rankName, fName;
            bool isArrive;

            try
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    List<MainUser> mainUsers = db.MainUsers.OrderBy(mu => mu.Num).ToList();
                    Team team = db.Teams.Find(1);

                    team.OnList = mainUsers.Count;                    

                    foreach (MainUser mainUser in mainUsers)
                    {
                        if (mainUser.StatusId == 1)
                        {
                            num = mainUser.Num;
                            usr = db.Users.Where(u => u.id == mainUser.UserId).FirstOrDefault();
                            fName = usr.Surname + " " + usr.Name.Substring(0, 1) + "." + usr.MiddleName.Substring(0, 1) + ".";
                            rank = db.Ranks.Where(r => r.id == usr.RankId).FirstOrDefault();
                            rankName = rank.rankName;
                            if (mainUser.ArriveStatus == 1)
                            {
                                isArrive = true;
                            }
                            else
                            {
                                isArrive = false;
                                noArrived++;
                            }

                            arriveUsers.Add(new ArriveUser(num, rankName, fName, isArrive));
                        }
                        else
                        {
                            if (mainUser.StatusId == 2)
                            {
                                onService += 1;
                            }
                            else
                            {
                                absent += 1;
                            }
                        }

                        if (mainUser.Ch10 != null) { ch10++; }
                        else if (mainUser.Ch15 != null) { ch15++; }
                        else if (mainUser.Ch20 != null) { ch20++; }
                    }

                    team.OnFace = team.OnList - absent - onService;
                    team.OnService = onService;
                    team.Absent = absent;
                    team.ShouldCome = team.OnFace - ch10 - ch15 - ch20;
                    team.Ch10 = ch10;
                    team.Ch15 = ch15;
                    team.Ch20 = ch20;
                    team.NoArrived = noArrived;

                    db.SaveChanges();
                }

                GridUsers.ItemsSource = arriveUsers;
                GridUsers.SelectedIndex = selectedIndex;


                arriveUsers.ListChanged += ArriveUsers_ListChanged;
                RefreshGridTeams();                
            }
            catch (Exception)
            {
                MessageBox.Show("Возникла ошибка при работе с базой данных. Продолжение невозможно.");
                Close();
            }
        }
        private void GridUsers_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshGridUsers();
        }

        private void CheckArrive(DateTime currentTime)
        {
            try
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    ArriveUser selectedRow = GridUsers.SelectedItem as ArriveUser;
                    MainUser mainUser = db.MainUsers.Where(mu => mu.Num == selectedRow.num).FirstOrDefault();
                    Team team = db.Teams.Find(1);

                    if (currentTime < Convert.ToDateTime("01:00:00"))
                    {
                        mainUser.Ch10 = currentTime.ToString(@"hh\:mm\:ss");
                        team.Ch10++;
                        team.Ch15++;
                        team.Ch20++;
                        team.NoArrived--;
                    }
                    else if (currentTime < Convert.ToDateTime("01:30:00") && (currentTime > Convert.ToDateTime("01:00:00")))
                    {
                        mainUser.Ch15 = currentTime.ToString(@"hh\:mm\:ss");
                        team.Ch15++;
                        team.Ch20++;
                        team.NoArrived--;
                    }
                    else if (currentTime < Convert.ToDateTime("02:00:00"))
                    {                        
                        mainUser.Ch20 = currentTime.ToString(@"hh\:mm\:ss");
                        team.Ch20++;
                        team.NoArrived--;
                    }                    

                    db.SaveChanges();                    
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Возникла ошибка при работе с базой данных. Продолжение невозможно.");
                Close();
            }            
        }
        private void UncheckArrive()
        {
            try
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    ArriveUser selectedRow = GridUsers.SelectedItem as ArriveUser;
                    MainUser mainUser = db.MainUsers.Where(mu => mu.Num == selectedRow.num).FirstOrDefault();
                    Team team = db.Teams.Find(1);


                    if (mainUser.Ch10 != null)
                    {
                        mainUser.Ch10 = null;
                        team.Ch10--;
                        team.Ch15--;
                        team.Ch20--;
                        team.NoArrived++;
                    } else if (mainUser.Ch15 != null)
                    {
                        mainUser.Ch15 = null;
                        team.Ch15--;
                        team.Ch20--;
                        team.NoArrived++;
                    }
                    else if (mainUser.Ch20 != null)
                    {
                        mainUser.Ch20 = null;
                        team.Ch20--;
                        team.NoArrived++;
                    }                    
                   
                    db.SaveChanges();
                }                
            }
            catch (Exception)
            {
                MessageBox.Show("Возникла ошибка при работе с базой данных. Продолжение невозможно.");
                Close();
            }
        }

        private void ArriveUsers_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                ArriveUser selectedRow = GridUsers.SelectedItem as ArriveUser;
                try
                {
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        MainUser mainUser = db.MainUsers.Where(mu => mu.Num == selectedRow.num).FirstOrDefault();
                        if (mainUser != null)
                        {
                            DateTime currentTime = DateTime.Now;
                            if (selectedRow.isArrive)
                            {
                                mainUser.arriveStatus = 1;
                                mainUser.time = LableTime.Content.ToString();
                                currentTime = Convert.ToDateTime((Convert.ToDateTime(mainUser.time) - Flags.ConvertedTime)
                                                                  .ToString(@"hh\:mm\:ss"));
                                CheckArrive(currentTime);
                            }
                            else
                            {
                                currentTime = Convert.ToDateTime((Convert.ToDateTime(mainUser.time) - Flags.ConvertedTime)
                                                                  .ToString(@"hh\:mm\:ss"));
                                UncheckArrive();
                                mainUser.arriveStatus = 0;
                                mainUser.time = string.Empty;
                            }
                        }
                        db.SaveChanges();
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Возникла ошибка при работе с базой данных. Продолжение невозможно.");
                    Close();
                }
                RefreshGridTeams();
            }
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            labelUprNoArrive.Content = labelUprShouldCome.Content;

            Flags.ConvertedTime = Convert.ToDateTime(TextBoxHours.Text + ":" + TextBoxMinutes.Text);
            TextBoxHours.IsEnabled = false;
            TextBoxMinutes.IsEnabled = false;
            ButtonStart.IsEnabled = false;
            //ButtonAddTeam.IsEnabled = false;
            //ButtonRemTeam.IsEnabled = false;
            //ButtonEditUsers.IsEnabled = false;
            ButtonParse.IsEnabled = true;
            ButtonCh10.IsEnabled = true;
            ButtonCh15.IsEnabled = true;
            ButtonCh20.IsEnabled = true;
            ButtonArrive.IsEnabled = true;
            ButtonGoodReason.IsEnabled = true;
            ButtonNoArrive.IsEnabled = true;

            LabelArriveCh10.Content = "0";
            LabelArriveCh15.Content = "0";
            LabelArriveCh20.Content = "0";
            LabelPercentCh10.Content = "0,00%";
            LabelPercentCh15.Content = "0,00%";
            LabelPercentCh20.Content = "0,00%";
            labelUprNoArrive.Content = labelUprShouldCome.Content;

            try
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    foreach (MainUser mainUser in db.MainUsers)
                    {
                        mainUser.ArriveStatus = 0;
                        mainUser.time = string.Empty;
                        mainUser.Ch10 = null;
                        mainUser.Ch15 = null;
                        mainUser.Ch20 = null;
                    }

                    foreach (Team team in db.Teams)
                    {
                        team.Ch10 = 0;
                        team.Ch15 = 0;
                        team.Ch20 = 0;
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Возникла ошибка при работе с базой данных. Продолжение невозможно.");
                Close();
            }
            RefreshGridUsers();
            Flags.isStarted = true;
        }

        private void ComboBoxRank_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (ApplicationContext db = new ApplicationContext()) ///Данные в ComboBoxRank из таблицы Ranks
                {
                    ComboBoxRank.ItemsSource = db.Ranks.Where(r => r.id >= 18).ToList();
                    ComboBoxRank.DisplayMemberPath = "rankName";
                }
            }
            catch (Exception)
            {

                MessageBox.Show("Возникла ошибка при работе с базой данных. Продолжение невозможно.");
                Close();
            }
        }

        private void TextBoxName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxName.Text == "Фамилия И.О.") ///Убираем подсказку с текстового поля
            {
                TextBoxName.Text = string.Empty;
                TextBoxName.Foreground = Brushes.Black;
            }
        }

        private void TextBoxName_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((TextBoxName.Text == string.Empty) || (TextBoxName.Text == " "))
            { ///Устанавливаем значение по умолчанию, если пользователь ничего не ввел
                TextBoxName.Text = "Фамилия И.О.";
                TextBoxName.Foreground = Brushes.Gray;
            }
        }

        private void ComboBoxRank_DropDownOpened(object sender, EventArgs e)
        {
            if (ComboBoxRank.SelectedIndex == -1) ///Делаем текст черным для выбора в/звания
            {
                ComboBoxRank.Foreground = Brushes.Black;
            }
        }

        private void ComboBoxRank_DropDownClosed(object sender, EventArgs e)
        {
            if (ComboBoxRank.SelectedIndex == -1) ///Возвращаем серый цвет тексту, если в/звание не выбрано
            {
                ComboBoxRank.Foreground = Brushes.Gray;
            }
        }

        private void TextBoxName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) ///Запрет на ввод более 1-го пробела подряд
            {
                if (!string.IsNullOrEmpty(TextBoxName.Text)) ///Если строка пустая, то просто вводим пробел
                {
                    string chkSpace = TextBoxName.Text.Substring(TextBoxName.Text.Length - 1, 1);
                    if (chkSpace == " ")
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        private void MenuEditTeams_Click(object sender, RoutedEventArgs e)
        {
            EditTableWindow editTableWindow = new EditTableWindow();
            editTableWindow.ShowDialog();
        }

        private void RefreshGridTeams()
        {
            int selectedIndex = DataGridTeam.SelectedIndex;
            BindingList<Team> teams = new BindingList<Team>();
            try
            {
                int onList = 0, onFace = 0, onService = 0, absent = 0, ch10 = 0, ch15 = 0, ch20 = 0, noArrived = 0, shouldCome = 0;

                using (ApplicationContext db = new ApplicationContext()) ///Данные в DataGridTeam из таблицы Teams
                {
                    foreach (Team team in db.Teams)
                    {
                        if (team.id != 1)
                        {
                            teams.Add(team);                            
                        } else
                        {
                            labelUprOnList.Content = team.OnList.ToString();
                            labelUprOnFace.Content = team.OnFace.ToString();
                            labelUprOnService.Content = team.OnService.ToString();
                            labelUprAbsent.Content = team.Absent.ToString();
                            labelUprCh10.Content = team.Ch10.ToString();                            
                            labelUprCh15.Content = team.Ch15.ToString();
                            labelUprCh20.Content = team.Ch20.ToString();
                            labelUprNoArrive.Content = team.NoArrived.ToString();
                            labelUprShouldCome.Content = team.ShouldCome.ToString();
                        }

                        onList += team.OnList;
                        onFace += team.OnFace;
                        onService += team.OnService;
                        absent += team.Absent;
                        ch10 += team.Ch10;
                        ch15 += team.Ch15;
                        ch20 += team.Ch20;
                        noArrived += team.NoArrived;
                        shouldCome += team.ShouldCome;

                    }

                    DataGridTeam.ItemsSource = teams;

                    labelOnList.Content = onList.ToString();
                    labelOnList.Content = onList.ToString();
                    labelOnFace.Content =onFace.ToString();
                    labelOnService.Content = onService.ToString();
                    labelAbsent.Content = absent.ToString();
                    labelCh10.Content = ch10.ToString();
                    LabelArriveCh10.Content = ch10.ToString();
                    labelCh15.Content = ch15.ToString();
                    LabelArriveCh15.Content = ch15.ToString();
                    labelCh20.Content = ch20.ToString();
                    LabelArriveCh20.Content = ch20.ToString();
                    labelNoArrive.Content = noArrived.ToString();
                    labelShouldCome.Content = shouldCome.ToString();

                    

                    FindPercent(LabelPercentCh10, LabelArriveCh10, labelShouldCome);
                    FindPercent(LabelPercentCh15, LabelArriveCh15, labelShouldCome);
                    FindPercent(LabelPercentCh20, LabelArriveCh20, labelShouldCome);
                }
                DataGridTeam.SelectedIndex = selectedIndex;

            }
            catch (Exception)
            {
                MessageBox.Show("Возникла ошибка при работе с базой данных. Продолжение невозможно.");
                Close();
            }
            teams.ListChanged += Teams_ListChanged;
        }
        private void DataGridTeam_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshGridTeams();
        }

        private void Teams_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                Team selectedRow = DataGridTeam.SelectedItem as Team;

                try
                {
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        Team team = db.Teams.Find(selectedRow.id);
                        if (team != null)
                        {
                            team.TeamName = selectedRow.teamName;
                            team.OnList = selectedRow.OnList;
                            team.OnService = selectedRow.OnService;
                            team.Absent = selectedRow.Absent;
                            team.OnFace = team.OnList - team.Absent;
                            selectedRow.ShouldCome = team.OnFace - team.OnService;
                            team.ShouldCome = selectedRow.ShouldCome;

                            SummLabelInt(LabelArriveCh10, LabelArriveCh10, -team.Ch10);
                            SummLabelInt(LabelArriveCh15, LabelArriveCh15, -team.Ch15);
                            SummLabelInt(LabelArriveCh20, LabelArriveCh20, -team.Ch20);

                            team.Ch10 = selectedRow.Ch10;
                            if (selectedRow.Ch15 < team.Ch10) { team.Ch15 = team.Ch10; }
                            else { team.Ch15 = selectedRow.Ch15; }
                            if (selectedRow.Ch20 < team.Ch15) { team.Ch20 = team.Ch15; }
                            else { team.Ch20 = selectedRow.Ch20; }

                            SummLabelInt(LabelArriveCh10, LabelArriveCh10, team.Ch10);
                            SummLabelInt(LabelArriveCh15, LabelArriveCh15, team.Ch15);
                            SummLabelInt(LabelArriveCh20, LabelArriveCh20, team.Ch20);

                            FindPercent(LabelPercentCh10, LabelArriveCh10, labelShouldCome);
                            FindPercent(LabelPercentCh15, LabelArriveCh15, labelShouldCome);
                            FindPercent(LabelPercentCh20, LabelArriveCh20, labelShouldCome);

                            team.NoArrived = selectedRow.ShouldCome - team.Ch20;
                            //db.Entry(team).State = EntityState.Modified;
                        }
                        db.SaveChanges();
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Возникла ошибка при работе с базой данных. Продолжение невозможно.");
                    Close();
                }
            }
            if (e.ListChangedType == ListChangedType.ItemDeleted)
            {
                try
                {
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        db.SaveChanges();
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Возникла ошибка при работе с базой данных. Продолжение невозможно.");
                    Close();
                }
            }
        }

        Regex inputRegex = new Regex(@"^[0-9]$");
        private void TextBoxHours_PreviewTextInput(object sender, TextCompositionEventArgs e) ///Ввод только цифр и не более 2-х
        {
            Match match = inputRegex.Match(e.Text);
            if ((sender as TextBox).Text.Length >= 2 || !match.Success)
            {
                e.Handled = true;
            }
        }

        private void TextBoxMinutes_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Match match = inputRegex.Match(e.Text);
            if ((sender as TextBox).Text.Length >= 2 || !match.Success)
            {
                e.Handled = true;
            }
        }

        private void TextBoxHours_KeyUp(object sender, KeyEventArgs e) ///24-часовой формат
        {
            if (TextBoxHours.Text != string.Empty && Convert.ToInt16(TextBoxHours.Text) > 23)
            {
                TextBoxHours.Text = TextBoxHours.Text.Remove(TextBoxHours.Text.Length - 1);
                TextBoxHours.SelectionStart = TextBoxHours.Text.Length;
            }

        }

        private void TextBoxMinutes_KeyUp(object sender, KeyEventArgs e)///24-часовой формат
        {
            if (TextBoxMinutes.Text != string.Empty && Convert.ToInt16(TextBoxMinutes.Text) > 59)
            {
                TextBoxMinutes.Text = TextBoxMinutes.Text.Remove(TextBoxMinutes.Text.Length - 1);
                TextBoxMinutes.SelectionStart = TextBoxMinutes.Text.Length;
            }
        }

        private void TextBoxMinutes_PreviewKeyDown(object sender, KeyEventArgs e)///Запрет на ввод пробела
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        private void TextBoxHours_PreviewKeyDown(object sender, KeyEventArgs e)///Запрет на ввод пробела
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        private void TextBoxHours_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxHours.Text == "00")
                TextBoxHours.Text = string.Empty;
        }

        private void TextBoxHours_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxHours.Text == string.Empty)
                TextBoxHours.Text = "00";
            else if (TextBoxHours.Text.Length == 1)
                TextBoxHours.Text = "0" + TextBoxHours.Text;
        }

        private void TextBoxMinutes_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxMinutes.Text == "00")
                TextBoxMinutes.Text = string.Empty;
        }

        private void TextBoxMinutes_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxMinutes.Text == string.Empty)
                TextBoxMinutes.Text = "00";
            else if (TextBoxMinutes.Text.Length == 1)
                TextBoxMinutes.Text = "0" + TextBoxMinutes.Text;
        }

        private void ButtonParse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Текстовый файл (*.txt)|*.txt"
            };

            Nullable<bool> result = dialog.ShowDialog();

            if (result == true)
            {
                if (GridUsers.SelectedIndex == -1)
                {
                    GridUsers.SelectedIndex = 0;
                }
                string s;
                int i = 0;
                DateTime dt = DateTime.Now;
                StreamReader f = new StreamReader(dialog.FileName, Encoding.GetEncoding(1251));
                while ((s = f.ReadLine()) != null)
                {
                    if (0 == i) ///Проверка заголовков
                    {
                        if (s.Length < 57)
                        {
                            MessageBox.Show("Данный файл не подходит для вставки");
                            break;
                        }
                        else if (s.Substring(0, 36) != "Архивная справка по событиям системы")
                        {
                            MessageBox.Show("Данный файл не подходит для вставки");
                            break;
                        }
                    }

                    if (2 == i) ///Сверка времени из файла со временем подачи сигнала
                    {
                        try ///На случай если файл заполнен не корректно
                        {
                            dt = Convert.ToDateTime(s.Substring(35));
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Данный файл не подходит для вставки");
                            break;
                        }

                        if (dt < Flags.ConvertedTime)
                        {
                            MessageBox.Show("Данный файл не содержит сведений о прибытии после " + Flags.ConvertedTime.ToString());
                            break;
                        }
                    }

                    if (10 < i)
                    {
                        ParsedString parsedString = new ParsedString();
                        if (s.Length < 72) { continue; } ///Минимальная корректная длина строки для обработки
                        try ///Проверка на значение начала строки типа DateTime
                        {
                            parsedString.arriveTime = Convert.ToDateTime(s.Substring(0, 17));
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        if (parsedString.arriveTime >= Flags.ConvertedTime)
                        {
                            parsedString.timeAfterSignal = Convert.ToDateTime((parsedString.arriveTime - Flags.ConvertedTime)
                                                                  .ToString(@"hh\:mm\:ss"));
                            parsedString.passType = s.Substring(20, 4);
                        }
                        else { continue; }

                        if (parsedString.passType == "ВХОД")
                        {
                            parsedString.eventType = s.Substring(54, 15);
                        }
                        else { continue; }

                        if (parsedString.eventType == "Проход завершен")
                        {
                            s = s.Substring(71);
                            int pos = s.IndexOf(' ');
                            parsedString.surname = s.Substring(0, pos);
                            s = s.Substring(pos + 1);
                            pos = s.IndexOf(' ');
                            parsedString.name = s.Substring(0, pos);
                            s = s.Substring(pos + 1);
                            pos = s.IndexOf('.');
                            parsedString.middleName = s.Substring(0, pos);
                        }
                        else { continue; }
                        try
                        {
                            using (ApplicationContext db = new ApplicationContext()) ///
                            {
                                List<int> numbers = new List<int>(); 
                                List <User> users = db.Users.Where(u => u.Surname == parsedString.surname &&
                                                                u.Name == parsedString.name &&
                                                                u.MiddleName == parsedString.middleName).ToList();
                                User user = users.First();
                                MainUser mainUser = new MainUser();
                                if (users.Count > 1)
                                {
                                    foreach (User usr in users)
                                    {
                                        mainUser = db.MainUsers.Where(mu => mu.UserId == usr.id).FirstOrDefault();
                                        if (mainUser != null)
                                        {
                                            numbers.Add(mainUser.Num);
                                        }
                                    }
                                    ChooseUserWindow chooseUserWindow = new ChooseUserWindow(parsedString.surname, parsedString.name, parsedString.middleName, numbers);
                                    chooseUserWindow.ShowDialog();
                                    if (Flags.selectedIndex >= 0)
                                    {
                                        user = users[Flags.selectedIndex];
                                    }
                                }
                                
                                if (user != null)
                                {
                                    mainUser = db.MainUsers.Where(mu => mu.UserId == user.id).FirstOrDefault();
                                    if (mainUser != null)
                                    {
                                        if (mainUser.ArriveStatus == 1)
                                        {
                                            dt = Convert.ToDateTime(mainUser.Time);
                                            if (parsedString.arriveTime < dt)
                                            {
                                                dt = Convert.ToDateTime((dt - Flags.ConvertedTime).ToString(@"hh\:mm\:ss"));
                                                UncheckArrive();
                                                mainUser.Time = parsedString.arriveTime.ToString();
                                                CheckArrive(parsedString.timeAfterSignal);
                                            }
                                        }
                                        else
                                        {
                                            mainUser.ArriveStatus = 1;
                                            mainUser.Time = parsedString.arriveTime.ToString();
                                            CheckArrive(parsedString.timeAfterSignal);
                                        }
                                    }
                                }
                                db.SaveChanges();
                                RefreshGridUsers();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);

                            MessageBox.Show("Возникла ошибка при работе с базой данных. Строка не добавлена.");
                            continue;
                        }
                    }
                    i++;
                }
                f.Close();
            }
        }

        private void ButtonAddTeam_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (ApplicationContext db = new ApplicationContext()) ///
                {
                    Team team = new Team();
                    db.Teams.Add(team);
                    db.SaveChanges();
                    RefreshGridTeams();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                MessageBox.Show("Возникла ошибка при работе с базой данных. Строка не добавлена.");
            }
        }

        private void ButtonRemTeam_Click(object sender, RoutedEventArgs e)
        {
            Team selectedRow = DataGridTeam.SelectedItem as Team;
            if (selectedRow != null && DataGridTeam.Items.Count > 1)
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить данное подразделение?\n\n"
                                                           + selectedRow.TeamName + "\n\n"
                                                           + "Отменить данный выбор будет НЕВОЗМОЖНО!",
                                                           "Проверка данных.Удаление последней записи",
                                                           MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (ApplicationContext db = new ApplicationContext()) ///
                        {
                            Team team = db.Teams.Find(selectedRow.id);

                            db.Teams.Remove(team);
                            db.SaveChanges();
                            RefreshGridTeams();
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Возникла ошибка при работе с базой данных. Строка не удалена.");
                    }
                }                    
            }
            else
            {
                MessageBox.Show("Строка не выбрана, либо является последней в таблице. Удаление НЕВОЗМОЖНО!");
            }
        }

        private void ButtonEditUsers_Click(object sender, RoutedEventArgs e)
        {
            EditTableWindow editTableWindow = new EditTableWindow();
            editTableWindow.ShowDialog();
            RefreshGridUsers();            
        }

        private void DataGridTeam_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            RefreshGridTeams();
        }

        private void WordReport (string type)
        {
            if (ComboBoxRank.SelectedIndex == -1 || TextBoxName.Text == "Фамилия И.О.")
            {
                MessageBox.Show("Введите в/зв и ФИО дежурного по части (Правый верхний угол)");
            }
            else
            {
                string arrived = "";
                string percent = "";
                string time = "";

                try
                {
                    using (ApplicationContext db = new ApplicationContext()) ///
                    {
                        List<Team> teams = db.Teams.ToList();

                        Application app = new Application();
                        Document wordDoc = app.Documents.Add(Visible: true);


                        if (app.Options.Overtype)
                        {
                            app.Options.Overtype = false;
                        }

                        Range title = wordDoc.Paragraphs[1].Range;
                        Range currentRange = wordDoc.Paragraphs.Last.Range;

                        switch (type)
                        {
                            case "ch10":
                                title.Text = "Отчет о прибытии на Ч+1.00\n";
                                break;
                            case "ch15":
                                title.Text = "Отчет о прибытии на Ч+1.30\n";
                                break;
                            case "ch20":
                                title.Text = "Отчет о прибытии на Ч+2.00\n";
                                break;                            
                            case "arrived":
                                title.Text = "Отчет о прибытии\nВремя с подачи сигнала: " + LableSignalTime.Content;
                                wordDoc.Paragraphs.Add();
                                break;
                            case "noArrived":
                                time = "Список не прибывших от управления\nВремя с подачи сигнала: " + LableSignalTime.Content;
                                wordDoc.Paragraphs.Add();
                                title.Text = time;
                                break;
                            case "goodReason":
                                title.Text = "Список отсутствующих по уважительной причине";
                                break;
                            default:
                                break;
                        }

                        title.Font.Size = 14;
                        title.Font.Name = "Times New Roman";
                        title.Font.Bold = 1;
                        title.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                        title.ParagraphFormat.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
                        title.ParagraphFormat.SpaceAfter = 0;
                        wordDoc.Paragraphs.Add();

                        if (type != "noArrived" && type != "goodReason") /// Таблица прибытия с процентами
                        {
                            Range titelTeams = wordDoc.Paragraphs.Last.Range;
                            titelTeams.Text = "Подразделения:";
                            titelTeams.Font.Size = 12;
                            titelTeams.Font.Name = "Times New Roman";
                            titelTeams.Font.Bold = 1;
                            titelTeams.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphJustify;
                            wordDoc.Paragraphs.Add();

                            currentRange = wordDoc.Paragraphs.Last.Range;
                            currentRange.Select();
                            currentRange.Font.Bold = 0;
                            currentRange.Font.Size = 12;

                            Table tableTeams = wordDoc.Tables.Add(currentRange, 1, 4);
                            tableTeams.Columns[2].Width = 50;
                            tableTeams.AutoFitBehavior(WdAutoFitBehavior.wdAutoFitWindow);

                            tableTeams.Cell(1, 1).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                            tableTeams.Cell(1, 2).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;

                            tableTeams.Cell(1, 3).Range.Font.Bold = 1;
                            tableTeams.Cell(1, 3).Range.Font.Size = 24;
                            tableTeams.Cell(1, 3).VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                            tableTeams.Cell(1, 4).Range.Font.Bold = 1;
                            tableTeams.Cell(1, 4).Range.Font.Size = 24;
                            tableTeams.Cell(1, 4).VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                            switch (type)
                            {
                                case "ch10":
                                    arrived = LabelArriveCh10.Content.ToString();
                                    percent = LabelPercentCh10.Content.ToString();
                                    break;
                                case "ch15":
                                    arrived = LabelArriveCh15.Content.ToString();
                                    percent = LabelPercentCh15.Content.ToString();
                                    break;
                                default:
                                    arrived = LabelArriveCh20.Content.ToString();
                                    percent = LabelPercentCh20.Content.ToString();
                                    break;
                            }
                            foreach (Team team in teams)
                            {
                                tableTeams.Cell(1, 1).Range.Text += team.TeamName;

                                switch (type)
                                {
                                    case "ch10":
                                        tableTeams.Cell(1, 2).Range.Text += "- " + team.Ch10;
                                        tableTeams.Cell(1, 3).Range.Text = "ВСЕГО: " + LabelArriveCh10.Content.ToString();
                                        tableTeams.Cell(1, 4).Range.Text = LabelPercentCh10.Content.ToString();
                                        break;
                                    case "ch15":
                                        tableTeams.Cell(1, 2).Range.Text += "- " + team.Ch15;
                                        tableTeams.Cell(1, 3).Range.Text = "ВСЕГО: " + LabelArriveCh15.Content.ToString();
                                        tableTeams.Cell(1, 4).Range.Text = LabelPercentCh15.Content.ToString();
                                        break;
                                    default:
                                        tableTeams.Cell(1, 2).Range.Text += "- " + team.Ch20;
                                        tableTeams.Cell(1, 3).Range.Text = "ВСЕГО: " + LabelArriveCh20.Content.ToString();
                                        tableTeams.Cell(1, 4).Range.Text = LabelPercentCh20.Content.ToString();
                                        break;
                                }
                            }

                            wordDoc.Paragraphs.Add();
                            Range titleArrived = wordDoc.Paragraphs.Last.Range;
                            titleArrived.Select();
                            titleArrived.Text = "Прибывшие от управления:\n";
                            titleArrived.Font.Bold = 1;
                            titleArrived.Font.Size = 12;
                        }

                        wordDoc.Paragraphs.Add();
                        currentRange = wordDoc.Paragraphs.Last.Range;
                        currentRange.Select();
                        currentRange.Font.Bold = 0;
                        currentRange.Font.Size = 10;
                        currentRange.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;

                        if (type == "goodReason" || type == "noArrived")
                        {
                            Table table = wordDoc.Tables.Add(currentRange, 1, 5);
                            table.Columns[1].Width = 50;
                            table.Columns[2].Width = 120;
                            table.AutoFitBehavior(WdAutoFitBehavior.wdAutoFitWindow);
                        }
                        else
                        {
                            Table table = wordDoc.Tables.Add(currentRange, 1, 6);
                            table.Columns[1].Width = 50;
                            table.Columns[2].Width = 120;
                            table.AutoFitBehavior(WdAutoFitBehavior.wdAutoFitWindow);
                        }

                        Table tableMain = wordDoc.Tables[wordDoc.Tables.Count];
                        tableMain.Columns[1].Width = 50;
                        tableMain.Columns[2].Width = 150;
                        tableMain.AutoFitBehavior(WdAutoFitBehavior.wdAutoFitWindow);

                        User user = new User();
                        Rank rank = new Rank();
                        Status status = new Status();

                        int i = 1;
                        foreach (MainUser mainUser in db.MainUsers)
                        {
                            if (mainUser != null)
                            { 
                                switch (type)
                                {
                                    case "ch10":
                                        if (mainUser.Ch10 != null)
                                        {
                                            user = db.Users.Find(mainUser.UserId);
                                            rank = db.Ranks.Find(user.RankId);
                                            tableMain.Rows.Add(tableMain.Rows[i]);
                                            tableMain.Rows[i].Borders[WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleSingle;
                                            tableMain.Cell(i, 1).Range.Text = i.ToString();
                                            tableMain.Cell(i, 2).Range.Text = rank.rankName;
                                            tableMain.Cell(i, 3).Range.Text = user.Surname;
                                            tableMain.Cell(i, 4).Range.Text = user.Name;
                                            tableMain.Cell(i, 5).Range.Text = user.middleName;
                                            tableMain.Cell(i, 6).Range.Text = mainUser.Ch10;
                                            i++;
                                        }
                                        break;
                                    case "ch15":
                                        if (mainUser.Ch15 != null)
                                        {
                                            user = db.Users.Find(mainUser.UserId);
                                            rank = db.Ranks.Find(user.RankId);
                                            tableMain.Rows.Add(tableMain.Rows[i]);
                                            tableMain.Rows[i].Borders[WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleSingle;
                                            tableMain.Cell(i, 1).Range.Text = i.ToString();
                                            tableMain.Cell(i, 2).Range.Text = rank.rankName;
                                            tableMain.Cell(i, 3).Range.Text = user.Surname;
                                            tableMain.Cell(i, 4).Range.Text = user.Name;
                                            tableMain.Cell(i, 5).Range.Text = user.middleName;
                                            tableMain.Cell(i, 6).Range.Text = mainUser.Ch15;
                                            i++;
                                        }
                                        break;
                                    case "ch20":
                                        if (mainUser.Ch20 != null)
                                        {
                                            user = db.Users.Find(mainUser.UserId);
                                            rank = db.Ranks.Find(user.RankId);
                                            tableMain.Rows.Add(tableMain.Rows[i]);
                                            tableMain.Rows[i].Borders[WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleSingle;
                                            tableMain.Cell(i, 1).Range.Text = i.ToString();
                                            tableMain.Cell(i, 2).Range.Text = rank.rankName;
                                            tableMain.Cell(i, 3).Range.Text = user.Surname;
                                            tableMain.Cell(i, 4).Range.Text = user.Name;
                                            tableMain.Cell(i, 5).Range.Text = user.middleName;
                                            tableMain.Cell(i, 6).Range.Text = mainUser.Ch20;
                                            i++;
                                        }
                                        break;
                                    case "arrived":
                                        if (mainUser.Ch10 != null || mainUser.Ch15 != null || mainUser.Ch20 != null)
                                        {
                                            user = db.Users.Find(mainUser.UserId);
                                            rank = db.Ranks.Find(user.RankId);
                                            tableMain.Rows.Add(tableMain.Rows[i]);
                                            tableMain.Rows[i].Borders[WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleSingle;
                                            tableMain.Cell(i, 1).Range.Text = i.ToString();
                                            tableMain.Cell(i, 2).Range.Text = rank.rankName;
                                            tableMain.Cell(i, 3).Range.Text = user.Surname;
                                            tableMain.Cell(i, 4).Range.Text = user.Name;
                                            tableMain.Cell(i, 5).Range.Text = user.MiddleName;
                                            tableMain.Cell(i, 6).Range.Text = mainUser.Time;
                                            i++;
                                        }
                                        break;
                                    case "noArrived":
                                        if (mainUser.StatusId == 1 &&
                                            mainUser.Ch10 is null && mainUser.Ch15 is null && mainUser.Ch20 is null)
                                        {
                                            user = db.Users.Find(mainUser.UserId);
                                            rank = db.Ranks.Find(user.RankId);
                                            tableMain.Rows.Add(tableMain.Rows[i]);
                                            tableMain.Rows[i].Borders[WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleSingle;
                                            tableMain.Cell(i, 1).Range.Text = i.ToString();
                                            tableMain.Cell(i, 2).Range.Text = rank.rankName;
                                            tableMain.Cell(i, 3).Range.Text = user.Surname;
                                            tableMain.Cell(i, 4).Range.Text = user.Name;
                                            tableMain.Cell(i, 5).Range.Text = user.MiddleName;
                                            i++;
                                        }
                                        break;
                                    case "goodReason":
                                        if (mainUser != null)
                                        {
                                            if (mainUser.StatusId != 1 && mainUser.StatusId != 6) //1 - На лицо, 6 - ВАКАНТ
                                            {
                                                user = db.Users.Find(mainUser.UserId);
                                                rank = db.Ranks.Find(user.RankId);
                                                status = db.Statuses.Find(mainUser.StatusId);
                                                tableMain.Rows.Add(tableMain.Rows[i]);
                                                tableMain.Rows[i].Borders[WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleSingle;
                                                tableMain.Cell(i, 1).Range.Text = i.ToString();
                                                tableMain.Cell(i, 2).Range.Text = rank.rankName;
                                                tableMain.Cell(i, 3).Range.Text = user.Surname;
                                                tableMain.Cell(i, 4).Range.Text = user.Name;
                                                tableMain.Cell(i, 5).Range.Text = user.MiddleName;
                                                tableMain.Cell(i, 5).Range.Text = status.statusName;
                                                i++;
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }                                
                            }
                        }

                        wordDoc.Paragraphs.Add();
                        currentRange = wordDoc.Paragraphs.Last.Range;
                        currentRange.Select();
                        string str = "Дежурный по части\n" + ComboBoxRank.Text + "\t______________\t\t" + TextBoxName.Text;
                        currentRange.Text = str;

                        wordDoc.Save();

                        try
                        {
                            wordDoc.Close();
                            app.Quit();

                        }
                        catch (Exception)
                        {
                            throw;
                        }

                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Возникла ошибка при работе с базой данных. Отчет не выгружен.");
                }
            }
        }

        private void ButtonCh10_Click(object sender, RoutedEventArgs e)
        {
            WordReport("ch10");
        }

        private void ButtonCh15_Click(object sender, RoutedEventArgs e)
        {
            WordReport("ch15");
        }

        private void ButtonCh20_Click(object sender, RoutedEventArgs e)
        {
            WordReport("ch20");
        }

        private void ButtonArrive_Click(object sender, RoutedEventArgs e)
        {
            WordReport("arrived");
        }

        private void ButtonNoArrive_Click(object sender, RoutedEventArgs e)
        {
            WordReport("noArrived");
        }

        private void ButtonGoodReason_Click(object sender, RoutedEventArgs e)
        {
            WordReport("goodReason");
        }

        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }
    }
}
