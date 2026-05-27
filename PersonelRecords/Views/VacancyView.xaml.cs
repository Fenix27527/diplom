using PersonelRecords.Modules;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PersonelRecords.Views
{
    public partial class VacancyView : UserControl
    {
        private MainWindow _mainWindow;
        private DataTable _vacancyTable;
        private DataTable _resumesTable;
        private DataTable _vacancyList; // Список вакансий для ComboBox

        // ✅ УБРАЛИ "Post" из полей резюме
        private readonly string[] _vacancyFields = new[] { "Post", "Conditions" };
        private readonly string[] _resumesFields = new[] { "FIO", "VacancyId", "Link" };

        public VacancyView(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        public void LoadData()
        {
            try
            {
                // 1. Загружаем таблицу Вакансии
                var (vTable, vAdapter) = ModuleDB.FillWithAdapter("SELECT * FROM Vacancy");
                _vacancyTable = vTable;
                TABLEVacancy.ItemsSource = _vacancyTable.DefaultView;

                // 2. Загружаем таблицу Резюме
                var (rTable, rAdapter) = ModuleDB.FillWithAdapter("SELECT * FROM Resumes");
                _resumesTable = rTable;
                TABLEResume.ItemsSource = _resumesTable.DefaultView;

                // 3. ✅ Загружаем список вакансий для ComboBox
                UpdateVacancyComboBoxList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TABLEResume_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.Column is DataGridComboBoxColumn comboBoxColumn && e.EditingElement is ComboBox comboBox)
            {
                comboBox.LostFocus += (s, args) =>
                {
                    if (comboBox.SelectedItem == null && !string.IsNullOrWhiteSpace(comboBox.Text))
                    {
                        MessageBox.Show("Выберите вакансию из списка!", "Ошибка ввода",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                };
            }
        }

        // ===== ВАКАНСИИ =====
        private void ButUpdVac_Click(object sender, RoutedEventArgs e)
        {
            ModuleHistory.SaveTableChanges(
                table: _vacancyTable,
                tableName: "Vacancy",
                historyTableName: "HISTORYVacancy",
                fieldsToTrack: _vacancyFields,
                currentUser: _mainWindow.CurrentUserFIO
            );
            UpdateVacancyComboBoxList();
        }

        private void ButDelVac_Click(object sender, RoutedEventArgs e)
        {
            ModuleHistory.DeleteSelectedRows(
                grid: TABLEVacancy,
                table: _vacancyTable,
                tableName: "Vacancy",
                historyTableName: "HISTORYVacancy",
                fieldsToTrack: _vacancyFields,
                currentUser: _mainWindow.CurrentUserFIO
            );
            UpdateVacancyComboBoxList();

            // После удаления вакансии обновляем список для ComboBox
            _vacancyList = ModuleDB.ExecuteSelect("SELECT Id, Post FROM Vacancy ORDER BY Post");
            if (VacancyComboBoxColumn != null)
            {
                VacancyComboBoxColumn.ItemsSource = _vacancyList.DefaultView;
            }
        }
        /// Обновляет список вакансий для ComboBox
        private void UpdateVacancyComboBoxList()
        {
            try
            {
                // 1. Загружаем актуальный список вакансий
                _vacancyList = ModuleDB.ExecuteSelect("SELECT Id, Post FROM Vacancy ORDER BY Post");

                // 2. Применяем к колонке
                if (VacancyComboBoxColumn != null)
                {
                    VacancyComboBoxColumn.ItemsSource = _vacancyList.DefaultView;
                }

                // 3. ✅ Принудительно обновляем отображение DataGrid
                var currentItemsSource = TABLEResume.ItemsSource;
                TABLEResume.ItemsSource = null;
                TABLEResume.ItemsSource = currentItemsSource;

                // 4. ✅ Обновляем данные в таблице (чтобы перечитать VacancyId из БД)
                var (updatedResumes, _) = ModuleDB.FillWithAdapter("SELECT * FROM Resumes");
                _resumesTable = updatedResumes;
                TABLEResume.ItemsSource = _resumesTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления списка вакансий: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButHistoryVac_Click(object sender, RoutedEventArgs e)
        {
            var hw = new HistoryWindow("HISTORYVacancy");
            hw.Owner = Window.GetWindow(this);
            hw.ShowDialog();
        }

        // ===== РЕЗЮМЕ =====
        private void ButUpdRes_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateResumesVacancyId())
            {
                MessageBox.Show("Исправьте ошибки в резюме перед сохранением", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ModuleHistory.SaveTableChanges(
                table: _resumesTable,
                tableName: "Resumes",
                historyTableName: "HISTORYResumes",
                fieldsToTrack: _resumesFields,
                currentUser: _mainWindow.CurrentUserFIO
            );
        }

        private bool ValidateResumesVacancyId()
        {
            foreach (DataRow row in _resumesTable.Rows)
            {
                if (row.RowState == DataRowState.Unchanged) continue;

                var vacancyId = row["VacancyId"];
                if (vacancyId == null || vacancyId == DBNull.Value)
                {
                    MessageBox.Show(
                        $"В строке {row["Id"]} не выбрана вакансия!\nВыберите вакансию из выпадающего списка.",
                        "Ошибка ввода",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return false;
                }
            }
            return true;
        }

        private void ButDelRes_Click(object sender, RoutedEventArgs e)
        {
            ModuleHistory.DeleteSelectedRows(
                grid: TABLEResume,
                table: _resumesTable,
                tableName: "Resumes",
                historyTableName: "HISTORYResumes",
                fieldsToTrack: _resumesFields,
                currentUser: _mainWindow.CurrentUserFIO
            );
        }

        private void ButHistoryRes_Click(object sender, RoutedEventArgs e)
        {
            var hw = new HistoryWindow("HISTORYResumes");
            hw.Owner = Window.GetWindow(this);
            hw.ShowDialog();
        }

        // ✅ УБРАЛИ "Post" из поиска по резюме
        private void ButSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Поиск по вакансиям
                ModuleSearch.Filter(_vacancyTable, TbSearch.Text, "Post", "Conditions");
                TABLEVacancy.ItemsSource = null;
                TABLEVacancy.ItemsSource = _vacancyTable.DefaultView;

                // Поиск по резюме
                ModuleSearch.Filter(_resumesTable, TbSearch.Text, "FIO", "Link");
                TABLEResume.ItemsSource = null;
                TABLEResume.ItemsSource = _resumesTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButBack_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow?.ShowMenu();
        }
    }
}