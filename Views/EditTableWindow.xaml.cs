using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.Entity;
using System.Globalization;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.Data;
using System.Data.SqlClient;
using Uchet.Classes;
using System.ComponentModel;
using System.Collections.ObjectModel;


namespace Uchet
{
    /// <summary>
    /// Логика взаимодействия для EditTableWindow.xaml
    /// </summary>



    public partial class EditTableWindow : Window
    {

        public static List<Status> statuses { get; private set; }
        public static List<Rank> ranks { get; private set; }

        public EditTableWindow()
        {
            InitializeComponent();

            try
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    statuses = db.Statuses.ToList();
                    ranks = db.Ranks.ToList();
                }

            }
            catch (Exception)
            {

                MessageBox.Show("Возникла ошибка при работе с базой данных. Окно будет закрыто.");
                Close();
            }

        }



        private void RefreshGridUsers()
        {
            ///Формирование основной таблицы Users

            int selectedIndex = GridTable.SelectedIndex;
            BindingList<EditUser> editUsers = new BindingList<EditUser>();
            Rank rank = null;
            User usr = null;
            Status status = null;
            String name, surname, middleName, statusName, rankName, position;
            int mainId, userId, num;

            try
            {
                using (ApplicationContext db = new ApplicationContext())
                {

                    List<MainUser> mainUsers = db.MainUsers.OrderBy(mu => mu.Num).ToList();

                    foreach (MainUser mainUser in mainUsers)
                    {
                        mainId = mainUser.id;
                        num = mainUser.Num;
                        userId = mainUser.UserId;
                        usr = db.Users.Where(u => u.id == userId).FirstOrDefault();
                        surname = usr.Surname;
                        name = usr.Name;
                        middleName = usr.MiddleName;
                        rank = db.Ranks.Where(r => r.id == usr.RankId).FirstOrDefault();
                        rankName = rank.rankName;
                        status = db.Statuses.Where(s => s.id == mainUser.StatusId).FirstOrDefault();
                        statusName = status.statusName;
                        position = usr.position;

                        editUsers.Add(new EditUser(mainId, userId, num, name, surname, middleName, statusName, rankName, position));
                    }
                }

                GridTable.ItemsSource = editUsers;


                statusColumn.ItemsSource = statuses;
                statusColumn.DisplayMemberPath = "statusName";
                ranksColumn.ItemsSource = ranks;
                ranksColumn.DisplayMemberPath = "rankName";

                editUsers.ListChanged += EditUsers_ListChanged;
                GridTable.SelectedIndex = selectedIndex;

            }
            catch (Exception)
            {

                MessageBox.Show("Возникла ошибка при работе с базой данных. Окно будет закрыто.");
                Close();
            }


        }

        private void EditUsers_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged)
            {

                EditUser selectedRow = GridTable.SelectedItem as EditUser;
                Rank rank = null;
                User user = null;
                Status status = null;
                bool isRefreshNeeded = false;
                try
                {
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        MainUser mainUser = db.MainUsers.Find(selectedRow.mainId);

                        if (mainUser != null)
                        {
                            user = db.Users.Where(u => u.id == mainUser.UserId).FirstOrDefault();
                            status = db.Statuses.Where(s => s.statusName == selectedRow.statusName).FirstOrDefault();
                            rank = db.Ranks.Where(r => r.rankName == selectedRow.rankName).FirstOrDefault();

                            user.Name = selectedRow.name;
                            user.Surname = selectedRow.surname;
                            user.MiddleName = selectedRow.middleName;
                            if (mainUser.StatusId != status.id)
                            {

                                if (status.id == 6)
                                {
                                    isRefreshNeeded = true;
                                    MessageBoxResult result = MessageBox.Show("Вы выбрали статус 'ВАКАНТ'. Очистить поле ФИО?", "Проверка данных.Вакант", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                                    if (result == MessageBoxResult.Yes)
                                    {
                                        user.Name = null;
                                        user.Surname = null;
                                        user.MiddleName = null;
                                    }
                                    mainUser.StatusId = status.id;
                                }
                                else
                                {

                                    if (selectedRow.name == null || selectedRow.surname == null || selectedRow.middleName == null)
                                    {
                                        MessageBox.Show("Введите ФИО");
                                    }
                                    else
                                    {
                                        mainUser.StatusId = status.id;
                                    }
                                }
                            }
                            user.Position = selectedRow.position;
                            user.RankId = rank.id;
                        }
                        db.SaveChanges();
                        if (isRefreshNeeded) { RefreshGridUsers(); }

                    }



                }
                catch (Exception)
                {
                    MessageBox.Show("Возникла ошибка при работе с базой данных. Окно будет закрыто.");
                    Close();
                }

            }
        }

        private void GridTable_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshGridUsers();


        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                db.SaveChanges();
                Close();

            }
        }

        private void GridTable_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            RefreshGridUsers();
        }

        private void Window_Closed(object sender, EventArgs e)
        {

            Application.Current.MainWindow.Show();

        }

        private void ButtonUp_Click(object sender, RoutedEventArgs e)
        {
            GridTable.CommitEdit();
            if (GridTable.SelectedIndex > 0)
            {
                EditUser selectedRow = GridTable.SelectedItem as EditUser;
                try
                {
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        MainUser mainUser = db.MainUsers.Find(selectedRow.mainId);

                        if (mainUser != null)
                        {
                            if (mainUser.Num > 1)
                            {
                                mainUser.Num -= 1;
                                GridTable.SelectedIndex -= 1;
                                selectedRow = GridTable.SelectedItem as EditUser;
                                mainUser = db.MainUsers.Find(selectedRow.mainId);
                                mainUser.Num += 1;
                            }

                        }
                        db.SaveChanges();
                        RefreshGridUsers();

                    }



                }
                catch (Exception)
                {
                    MessageBox.Show("Возникла ошибка при работе с базой данных. Окно будет закрыто.");
                    Close();
                }
            }
        }

        private void ButtonDown_Click(object sender, RoutedEventArgs e)
        {

            if (GridTable.SelectedIndex != GridTable.Items.Count - 1)
            {
                EditUser selectedRow = GridTable.SelectedItem as EditUser;
                try
                {
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        MainUser mainUser = db.MainUsers.Find(selectedRow.mainId);

                        if (mainUser != null)
                        {
                            mainUser.Num += 1;
                            GridTable.SelectedIndex += 1;
                            selectedRow = GridTable.SelectedItem as EditUser;
                            mainUser = db.MainUsers.Find(selectedRow.mainId);
                            mainUser.Num -= 1;
                        }
                        db.SaveChanges();
                        RefreshGridUsers();

                    }

                }
                catch (Exception)
                {
                    MessageBox.Show("Возникла ошибка при работе с базой данных. Окно будет закрыто.");
                    Close();
                }
            }
        }

        private void ButtonGoFirst_Click(object sender, RoutedEventArgs e)
        {
            if (GridTable.SelectedIndex > 0)
            {
                try
                {
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        EditUser selectedRow = GridTable.SelectedItem as EditUser;
                        MainUser currenUser = db.MainUsers.Find(selectedRow.mainId);
                        foreach (MainUser mainUser in db.MainUsers)
                        {
                            if (mainUser.Num < selectedRow.num)
                            {
                                mainUser.Num += 1;
                            }
                        }
                        currenUser.Num = 1;

                        db.SaveChanges();
                        GridTable.SelectedIndex = 0;
                        RefreshGridUsers();


                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Возникла ошибка при работе с базой данных. Окно будет закрыто.");
                    Close();
                }



            }
        }

        private void ButtonGoLast_Click(object sender, RoutedEventArgs e)
        {
            if (GridTable.SelectedIndex != GridTable.Items.Count - 1)
            {
                try
                {
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        EditUser selectedRow = GridTable.SelectedItem as EditUser;
                        MainUser currenUser = db.MainUsers.Find(selectedRow.mainId);
                        int i = 0;
                        foreach (MainUser mainUser in db.MainUsers)
                        {
                            if (mainUser.Num > selectedRow.num)
                            {
                                mainUser.Num -= 1;
                                i++;
                            }
                        }
                        currenUser.Num += i;

                        db.SaveChanges();

                        GridTable.SelectedIndex = GridTable.Items.Count - 1;
                        RefreshGridUsers();


                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Возникла ошибка при работе с базой данных. Окно будет закрыто.");
                    Close();
                }
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    GridTable.SelectedIndex = GridTable.Items.Count - 1;
                    EditUser selectedRow = GridTable.SelectedItem as EditUser;
                    int num = selectedRow.num + 1;
                    User user = new User(1);
                    db.Users.Add(user);
                    db.SaveChanges();
                    user = db.Users.OrderBy(u => u.id).ToList().Last();
                    MainUser mainUser = new MainUser(user.id, num, 6);
                    db.MainUsers.Add(mainUser);
                    db.SaveChanges();
                    RefreshGridUsers();
                    GridTable.SelectedIndex = GridTable.Items.Count - 1;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Возникла ошибка при работе с базой данных. Окно будет закрыто.");
                Close();
            }
        }

        private void ButtonDelLast_Click(object sender, RoutedEventArgs e)
        {
            if (GridTable.Items.Count != 1)
            {
                GridTable.SelectedIndex = GridTable.Items.Count - 1;
                EditUser selectedRow = GridTable.SelectedItem as EditUser;
                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить данную запись?\n\n"
                                                            + selectedRow.num + ". "
                                                            + selectedRow.rankName + " "
                                                            + selectedRow.surname + " "
                                                            + selectedRow.name + " "
                                                            + selectedRow.middleName + "\n\n"
                                                            + "Отменить данный выбор будет НЕВОЗМОЖНО!",
                                                            "Проверка данных.Удаление последней записи",
                                                            MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (ApplicationContext db = new ApplicationContext())
                        {
                            MainUser mainUser = db.MainUsers.Find(selectedRow.mainId);
                            User user = db.Users.Find(mainUser.UserId);
                            db.Users.Remove(user);
                            db.MainUsers.Remove(mainUser);
                            db.SaveChanges();
                            GridTable.SelectedIndex = GridTable.Items.Count - 2;
                            RefreshGridUsers();
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Возникла ошибка при работе с базой данных. Окно будет закрыто.");
                        Close();
                    }
                }

            } else
            {
                MessageBox.Show("Нельзя удалить последний элемент в таблице!");
            }

        }
    }
}
