using SQLitePCL;
using System;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;

namespace CloudTechIot5
{
    //http://www.cnblogs.com/cloudtech
    //cloudtechesx@gmail.com
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        #region Fields
        //数据库名
        private string _dbName;
        //表名
        private string _tableName;
        //name字段的数据集合
        private string[] _names = { "IoT-1", "IoT-2", "IoT-3" };

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        private string _result;
        //操作结果
        public string Result
        {
            get
            {
                return _result;
            }

            set
            {
                _result = value;
                OnPropertyChanged("Result");
            }
        }

        #endregion

        #region Constructor

        public MainPage()
        {
            //初始化
            _result = "Processing...";
            _dbName = "IoT5DB.sdf";
            _tableName = "users";
            this.InitializeComponent();
            //简易MVVM框架
            this.DataContext = this;
            //创建数据库连接
            using (SQLiteConnection connection = CreateDbConnection())
            {
                //创建表
                CreateTable(connection);
                foreach (string name in _names)
                {
                    //插入数据
                    InsertRow(connection, name);
                }
                //更新第二条数据
                UpdateRow(connection, string.Format("IoT-{0}", DateTime.Now.ToString("dd-HH:mm:ss")), _names[1]);
                //删除第一条数据
                DeleteRow(connection, _names[0]);
            }
            //更新界面
            Result = "Completed...";
        }

        #endregion

        #region Methods

        private SQLiteConnection CreateDbConnection()
        {
            //创建连接
            SQLiteConnection connection = new SQLiteConnection(_dbName);
            if (null == connection)
            {
                throw new Exception("create db connection failed");
            }
            return connection;
        }

        private void CreateTable(SQLiteConnection connection)
        {
            //创建表
            string sql = string.Format("create table if not exists {0} (id integer primary key autoincrement,name text)", _tableName);
            using (ISQLiteStatement sqliteStatement = connection.Prepare(sql))
            {
                //执行语句
                sqliteStatement.Step();
            }
        }

        private void InsertRow(SQLiteConnection connection, string name)
        {
            //插入数据
            string sql = string.Format("insert into {0} (name) values (?)", _tableName);
            using (ISQLiteStatement sqliteStatement = connection.Prepare(sql))
            {
                //绑定参数
                sqliteStatement.Bind(1, name);
                //执行语句
                sqliteStatement.Step();
            }
        }

        private void UpdateRow(SQLiteConnection connection, string newName, string oldName)
        {
            string sql = string.Format("update {0} set name = ? where name = ?", _tableName);
            using (ISQLiteStatement sqliteStatement = connection.Prepare(sql))
            {
                //绑定参数
                sqliteStatement.Bind(1, newName);
                sqliteStatement.Bind(2, oldName);
                //执行语句
                sqliteStatement.Step();
            }
        }

        private void DeleteRow(SQLiteConnection connection, string name)
        {
            string sql = string.Format("delete from {0} where name = ?", _tableName);
            using (ISQLiteStatement sqliteStatement = connection.Prepare(sql))
            {
                //绑定参数
                sqliteStatement.Bind(1, name);
                //执行语句
                sqliteStatement.Step();
            }
        }

        public void OnPropertyChanged(string propertyName)
        {
            //MVVM依赖属性事件
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
