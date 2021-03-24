using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Manager
{
    public class UIManager
    {
        #region Methods

        public static SecureString EnterPassword()
        {
            SecureString password = new SecureString();
            Console.WriteLine("Enter password: ");

            ConsoleKeyInfo nextKey = Console.ReadKey(true);

            while (nextKey.Key != ConsoleKey.Enter)
            {
                if (nextKey.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password.RemoveAt(password.Length - 1);
                        // erase the last * as well
                        Console.Write(nextKey.KeyChar);
                        Console.Write(" ");
                        Console.Write(nextKey.KeyChar);
                    }
                }
                else
                {
                    password.AppendChar(nextKey.KeyChar);
                    Console.Write("*");
                }
                nextKey = Console.ReadKey(true);
            }
            return password;
        }

        public static int ClientHasCertUI()
        {
            int option = -1;
            do
            {
                Console.WriteLine("=============================");
                Console.WriteLine("1. Deposit Money");
                Console.WriteLine("2. Withdraw Money");
                Console.WriteLine("3. Withdraw SmartCard");
                Console.WriteLine("4. Reset SmartCard pin");
                Console.WriteLine("5. Exit.");
                Console.WriteLine("=============================");
                try
                {
                    option = int.Parse(Console.ReadLine());
                }
                catch
                {
                    option = -1;
                }
            } while (option < 1 || option > 5);

            return option;
        }

        public static int ManagerHasCertUI()
        {
            int option = -1;
            do
            {
                Console.WriteLine("=============================");
                Console.WriteLine("0. List all valid users.");
                Console.WriteLine("5. Exit.");
                Console.WriteLine("=============================");
                try
                {
                    option = int.Parse(Console.ReadLine());
                }
                catch
                {
                    option = -1;
                }
            } while (option != 0 && option != 5);

            return option;
        }

        public static int UserHasntCertUI()
        {
            int option = -1;
            do
            {
                Console.WriteLine("=============================");
                Console.WriteLine("1. Publish new SmartCard");
                Console.WriteLine("2. Exit.");
                Console.WriteLine("=============================");
                try
                {
                    option = int.Parse(Console.ReadLine());
                }
                catch
                {
                    option = -1;
                }
            } while (option != 1);

            return option;
        }

        public static int ATMHasCertUI()
        {
            int option = -1;
            do
            {
                Console.WriteLine("=============================");
                Console.WriteLine("ATM ready for clients.");
                Console.WriteLine("1. Exit.");
                Console.WriteLine("=============================");
                try
                {
                    option = int.Parse(Console.ReadLine());
                }
                catch
                {
                    option = -1;
                }
            } while (option != 1);

            return option;
        }

        public static int ATMHasntCertUI()
        {
            int option = -1;

            do
            {
                Console.WriteLine("=============================");
                Console.WriteLine("1. Publish new ATM Certificate");
                Console.WriteLine("2. Exit.");
                Console.WriteLine("=============================");
                try
                {
                    option = int.Parse(Console.ReadLine());
                }
                catch
                {
                    option = -1;
                }
            } while (option != 1 && option != 2);

            return option;
        }

        
        #endregion
    }
}
