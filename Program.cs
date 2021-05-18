using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoveFileFromDir
{
    public static class FileConf
    {
        public static string MailFrom { get; set; }
        public static string NameFrom { get; set; }
        public static string EmailTo { get; set; }
        public static string Tema { get; set; }
        public static string SmtpSever { get; set; }
        public static string SmtpPort { get; set; }
        public static string AvtName { get; set; }
        public static string AvtPass { get; set; }
        public static string DirFrom { get; set; }
        public static string DirTo { get; set; }
        public static bool bSSL { get; set; }
        public static string RootDirMess { get; set; }
        //{
        //    get { return MailF; }
        //    set { MailF = value; }
        //}

    }
    class Program
    {

        static void Main(string[] args)
        {
            GetConfig(); //счытываем конфигурацию
            watch();//запускаем прослушиватель каталога
            vertushka();//Запускаем цикл перекладывания файлов по всей директории dir_from
            Console.WriteLine("для того щоб вийти натисніть будь яку клавішу");
            Console.ReadKey();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////
        private static void vertushka()
        {
            // начало обработки
            Console.WriteLine("начата обработка каталога полностью");
            WriteLog(DateTime.Now + " начата обработка каталога полностью");
            //1
            DirectoryInfo d1 = new DirectoryInfo(FileConf.DirFrom);// @"C:\!Проба");
            if (!d1.Exists)
            {
                Console.WriteLine("каталог {0} откуда не существует", FileConf.DirFrom);
                Console.WriteLine("для выхода нажмите любую клавишу");
                Console.ReadKey();
                return;
            }
            foreach (DirectoryInfo di1 in d1.GetDirectories())
            {
                DirectoryInfo d2 = new DirectoryInfo(di1.FullName);
                foreach (DirectoryInfo di2 in d2.GetDirectories())
                {

                    DirectoryInfo f3 = new DirectoryInfo(di2.FullName);
                    foreach (var f in f3.GetFiles())
                    {

                        string path = FileConf.DirTo; // @"C:\!САЗС";
                        path = Path.Combine(path, di2.Name, di1.Name);//определяем путь
                        DirectoryInfo d3 = new DirectoryInfo(path);//создаем директории
                        if (!d3.Exists)//проверка на существование
                        {
                            d3.Create();
                        }
                        var fn = Path.Combine(f3.FullName, f.Name);
                        MoveFile(fn);

                    }
                }
            }
        }




        private static void watch()
        {
            //String[] arguments = Environment.GetCommandLineArgs();//определяем аргументы командной строки
            // создаем новый FileSystemWatcher и устанавливаем его свойства
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = FileConf.DirFrom;
            watcher.IncludeSubdirectories = true;
            //задаем тип отслеживаемых изменений
            watcher.NotifyFilter = NotifyFilters.LastWrite;
           //| NotifyFilters.FileName; //NotifyFilters.LastAccess | NotifyFilters.DirectoryName
            // добавляем обработчики событий
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnDel);
            watcher.Deleted += new FileSystemEventHandler(OnDel);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);
            // начинаем отслеживание
            watcher.EnableRaisingEvents = true;
        }
        // Определяем обработчики событий.
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // что делаем если файл изменен, создан или удален.
            Console.WriteLine("Файл " + e.FullPath + " " + e.ChangeType);
            WriteLog(DateTime.Now + " Файл " + e.FullPath + " " + e.ChangeType);
            MoveFile(e.FullPath);
        }

        private static void OnDel(object source, FileSystemEventArgs e)
        {
            // что делаем если файл удален.
            Console.WriteLine("Файл " + e.FullPath + " " + e.ChangeType);
            WriteLog(DateTime.Now + " Файл " + e.FullPath + " " + e.ChangeType);
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // что делаем если файл перемещен.
            Console.WriteLine("Файл: {0} перемещен в {1}", e.OldFullPath, e.FullPath);
            WriteLog(DateTime.Now + " Файл " + e.OldFullPath + " перемещен в " + e.FullPath);
        }
        // подпрограмма записи в фал
        private static void WriteLog(string zap)
        {
            String[] arguments = Environment.GetCommandLineArgs();//определяем аргументы командной строки
            var logdir = Path.Combine(Path.GetDirectoryName(arguments[0]), "LOG");
            DirectoryInfo Di = new DirectoryInfo(logdir);
            if (!Di.Exists)
            {
                Di.Create();
                vertushka();

            }
            var logName = Path.Combine(Path.GetDirectoryName(arguments[0]), "LOG", DateTime.Now.Date.ToShortDateString() + ".log");

            //var file = Path.Combine(Path.GetDirectoryName(arguments[0]),"LOG", "watcher.log");

            using (StreamWriter sw = new StreamWriter(logName, true, System.Text.Encoding.Default))
            {
                sw.WriteLine(zap);
            }
        }
        /////////////////////////////////////// Подпрограмма перемещения файла
        private static void MoveFile(string filename) //filename c полным путем
        {

            //String[] arguments = Environment.GetCommandLineArgs();//определяем аргументы командной строки
            string dir_dest = FileConf.DirTo;//аргумент каталог_куда

            FileInfo fi = new FileInfo(filename);
            if (fi.Exists)
            {

                var dname = Path.GetDirectoryName(filename);
                var di1 = dname.Substring(dname.LastIndexOf("\\") + 1);
                dname = Path.GetDirectoryName(dname);
                var di2 = dname.Substring(dname.LastIndexOf("\\") + 1);
                var path = Path.Combine(dir_dest, di1, di2, Path.GetFileName(filename));
                var ok = true; //переименовываем файл
                var k = 0;
                while (ok)
                {
                    FileInfo fi1 = new FileInfo(path);//проверяем на существование файла в конечной директории
                    if (fi1.Exists)
                    {
                        k = k + 1;
                        var i = path.LastIndexOf(".");
                        if (k > 1)
                        {
                            path = string.Concat(path.Remove(i - 3), "(", k.ToString(), ")", path.Substring(i, path.Length - i));
                        }
                        else
                        {
                            path = string.Concat(path.Remove(i), "(", k.ToString(), ")", path.Substring(i, path.Length - i));
                        }

                    }
                    else
                    {
                        DirectoryInfo d = new DirectoryInfo(fi1.DirectoryName);
                        if (!d.Exists)
                        {
                            d.Create();
                        }
                        ok = false;
                    }
                }

                try
                {

                    fi.MoveTo(path); //fi.CopyTo(path, true);
                    try
                    {
                        SendMess(path);
                    }
                    catch (Exception er)
                    {
                        Console.WriteLine("При отправке файла {0} возникла ошибка {1}", path, er.Message);
                        WriteLog((DateTime.Now + " При отправке файла " + path + " возникла ошибка " + er.Message));
                    }

                    Console.WriteLine("Скопирован файл " + filename + " в папку " + path);
                    WriteLog((DateTime.Now + " Скопирован файл " + filename + " в папку " + path));
                    
                }
                catch (Exception e)
                {

                    Console.WriteLine("При перемещении файла {0} возникла ошибка {1}", path, e.Message);
                    WriteLog((DateTime.Now + " При перемещении файла " + path + " возникла ошибка " + e.Message));
                }
                
            }
        }

        /////////////отправка почты
        private static void SendMess(string mess) //mess-сообщение
        {
            string dir = Path.GetDirectoryName(Path.GetDirectoryName(mess));
            dir = dir.Substring(dir.LastIndexOf("\\") + 1, (dir.Length - dir.LastIndexOf("\\") - 1));
            GetEmail(dir);
            // отправитель - устанавливаем адрес и отображаемое в письме имя
            MailAddress from = new MailAddress(FileConf.MailFrom, FileConf.NameFrom);
            // кому отправляем
            MailAddress to = new MailAddress(FileConf.EmailTo);
            // создаем объект сообщения
            MailMessage m = new MailMessage(from, to);
            // тема письма
            m.Subject = "Получен файл " + FileConf.RootDirMess + mess.Substring(FileConf.RootDirMess.Length-3, (mess.Length- FileConf.RootDirMess.Length)+3);
            // текст письма
            m.Body = "<h2> Получен файл " + FileConf.RootDirMess + mess.Substring(FileConf.RootDirMess.Length-3, (mess.Length- FileConf.RootDirMess.Length)+3)+" </h2>" ;
            // письмо представляет код html
            m.IsBodyHtml = true;
            // адрес smtp-сервера и порт, с которого будем отправлять письмо
            SmtpClient smtp = new SmtpClient(FileConf.SmtpSever, Int32.Parse(FileConf.SmtpPort));
            // логин и пароль
            smtp.Credentials = new NetworkCredential(FileConf.AvtName, FileConf.AvtPass);
            smtp.EnableSsl = FileConf.bSSL;
            smtp.Timeout = 20000;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            try
            {
                smtp.Send(m);
                WriteLog(DateTime.Now + "Сообщение отправлено на " + FileConf.EmailTo);
            }
            catch (Exception er)
            {
                WriteLog(DateTime.Now + "Сообщение не отправлено" + er.Message);
            }
        }

        /////////////////////////////

        private static void GetConfig() // чтение конфигурационного файла
        {
            String[] arguments = Environment.GetCommandLineArgs();//определяем аргументы командной строки
            string dir_run = Path.GetDirectoryName(arguments[0]);//директория запуска 
            WriteLog(DateTime.Now + "Считивается конфигурация ");
            FileInfo f = new FileInfo(Path.Combine(dir_run, "watcher.cnf"));
            if (!f.Exists)
            {
                using (StreamWriter swconf = new StreamWriter(Path.Combine(dir_run, "watcher.cnf"), false, System.Text.Encoding.Default))
                {
                    swconf.WriteLine("dir_from: ;");
                    swconf.WriteLine("dir_to: ;");
                    swconf.WriteLine("mail_from: ;");
                    swconf.WriteLine("mail_name_from: ;");
                    swconf.WriteLine("mail_to: ;");
                    swconf.WriteLine("mail_tema: ;");
                    swconf.WriteLine("smtp_server: ;");
                    swconf.WriteLine("smtp_server_port: ;");
                    swconf.WriteLine("smtp_avt_login: ;");
                    swconf.WriteLine("smtp_avt_password: ;");
                    swconf.WriteLine("smtp_SSL: false;");
                    swconf.WriteLine("rootdirmess: ;");
                    swconf.Dispose();
                }
                Console.WriteLine("заполните конфигурационный файл {0}",f.FullName);
                WriteLog(DateTime.Now + " заполните конфигурационный файл "+f.FullName);
                exit();
            }
            using (StreamReader swconf = new StreamReader(Path.Combine(dir_run, "watcher.cnf"), System.Text.Encoding.Default))
            {
                string line, line1, line2;
                while ((line = swconf.ReadLine()) != null)
                {
                    if ((!(line.IndexOf(":") == -1) | !(line.IndexOf(";") == -1)) | !(line.Length == 0))
                    {
                        line1 = line.Substring(0, line.IndexOf(":") + 1);
                        line2 = line.Substring(line.IndexOf(":") + 1, line.IndexOf(";") - line.IndexOf(":") - 1);
                        switch (line1.Trim())
                        {

                            case ("dir_from:"):
                                FileConf.DirFrom = line2.Trim();
                                break;
                            case ("dir_to:"):
                                FileConf.DirTo = line2.Trim();
                                break;
                            case ("mail_from:"):
                                FileConf.MailFrom = line2.Trim();
                                break;
                            case ("mail_name_from:"):
                                FileConf.NameFrom = line2.Trim();
                                break;
                            case ("mail_to:"):
                                FileConf.EmailTo = line2.Trim();
                                break;
                            case ("mail_tema:"):
                                FileConf.Tema = line2.Trim();
                                break;
                            case ("smtp_server:"):
                                FileConf.SmtpSever = line2.Trim();
                                break;
                            case ("smtp_server_port:"):
                                FileConf.SmtpPort = line2.Trim();
                                break;
                            case ("smtp_avt_login:"):
                                FileConf.AvtName = line2.Trim();
                                break;
                            case ("smtp_avt_password:"):
                                FileConf.AvtPass = line2.Trim();
                                break;
                            case ("smtp_SSL:"):
                                FileConf.bSSL = Convert.ToBoolean(line2.Trim());
                                break;
                            case ("rootdirmess:"):
                                FileConf.RootDirMess = line2.Trim();
                                break;
                            default:
                                WriteLog(DateTime.Now + "не Считано " + line1 + " " + line2);
                                break;
                        }
                        WriteLog(DateTime.Now + "Считано " + line1 + " " + line2);
                    }
                }
                WriteLog(DateTime.Now + "конфигурация закончена");
                try
                {
                    DirectoryInfo di = new DirectoryInfo(FileConf.DirFrom);
                    if (!di.Exists)
                    {
                        Console.WriteLine("Укажите коректную директорию откуда dir_from: в конфигурационном файле watcher.cnf");
                        WriteLog(DateTime.Now + "Укажите коректную директорию откуда dir_from: в конфигурационном файле watcher.cnf");
                        exit();
                    }
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine("Укажите коректную директорию откуда dir_from: в конфигурационном файле watcher.cnf причина " + e.Message);
                    WriteLog(DateTime.Now + "Укажите коректную директорию откуда dir_from: в конфигурационном файле watcher.cnf причина " + e.Message);
                    exit();
                }
                try
                {
                    DirectoryInfo di1 = new DirectoryInfo(FileConf.DirTo); ;
                    if (!di1.Exists)
                    {
                        di1.Create();
                        //Console.WriteLine("Укажите коректную директорию откуда dir_to: в конфигурационном файле watcher.cnf");
                        //WriteLog(DateTime.Now + "Укажите коректную директорию куда dir_to: в конфигурационном файле watcher.cnf");
                        //exit();
                    }
                }
                catch (Exception e1)
                {
                    Console.WriteLine("Укажите коректную директорию откуда dir_to: в конфигурационном файле watcher.cnf причина " + e1.Message);
                    WriteLog(DateTime.Now + "Укажите коректную директорию откуда dir_to: в конфигурационном файле watcher.cnf причина " + e1.Message);
                    exit();
                }
                
            }
        }
        private static void GetEmail(string dir) // чтение конфигурационного файла c e-mail адресами email.adr
        {
            String[] arguments = Environment.GetCommandLineArgs();//определяем аргументы командной строки
            string dir_run = Path.GetDirectoryName(arguments[0]);//директория запуска 
            WriteLog(DateTime.Now + "Считивается конфигурация e-mail ");
            int a = 0;
            string lineDef = "";
            FileInfo fmail = new FileInfo(Path.Combine(dir_run, "email.adr"));
            if (!fmail.Exists)
            {
                DirectoryInfo d1 = new DirectoryInfo(FileConf.DirTo);// @"C:\!Проба");
                
                foreach (DirectoryInfo di1 in d1.GetDirectories())
                {
                    using (StreamWriter swconf = new StreamWriter(Path.Combine(dir_run, "mail.adr"), false, System.Text.Encoding.Default))
                    {
                            swconf.WriteLine(di1.Name + ": ;");
                    }    
                }
                WriteLog(DateTime.Now + " Файл адресов не существует создан новый - ЗАПОЛНИТЕ "+fmail.FullName);
                return;
            }
            using (StreamReader swconf = new StreamReader(Path.Combine(dir_run, "email.adr"), System.Text.Encoding.Default))
            {
                string line, line1, line2;
                while ((line = swconf.ReadLine()) != null)
                {
                    if ((!(line.IndexOf(":")==-1)| !(line.IndexOf(";")==-1))| !(line.Length == 0))
                   {
                    line1 = line.Substring(0, line.IndexOf(":"));
                    line2 = line.Substring(line.IndexOf(":") + 1, line.IndexOf(";") - line.IndexOf(":") - 1);
                    if (String.Compare(dir, line1, true) == 0)
                    {
                        FileConf.EmailTo = line2;
                        WriteLog(DateTime.Now + " Адрес для " + line1 + " " + line2);
                        a = 1;
                    }
                    if (String.Compare("по умолчанию", line1, true) == 0)
                    {
                        lineDef = line2;
                    }
                    }
                }
            }
            if (a == 0)
            {
                FileConf.EmailTo = lineDef;
            }

        }
        private static void exit()
        {
            Console.WriteLine("для того щоб вийти натисніть будь яку клавішу");
            Console.ReadKey();
            System.Environment.Exit(0);
        }
        
    }
}


   

  
 
