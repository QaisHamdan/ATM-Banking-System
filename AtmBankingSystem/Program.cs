using System;
using System.Linq;
using System.Collections.Generic;

namespace AtmBankingSystem
{
    class Program
    {
        private static decimal totalDeposited = 0.00m;
        private static decimal totalWithdrawn = 0.00m;
        private static decimal totalTransferred = 0.0m;

        // الرصيد المبدئي كمتغير عام
        private static decimal balance = 750.00m;
        private static string correctAccount = "123456";
        private static string correctPin = "1234";

        // حد السحب اليومي (500 دينار)
        const decimal DAILY_WITHDRAWAL_LIMIT = 500.00m;
        static decimal withdrawnToday = 0.00m;

        // تخزين أسطر السجل داخله
        private static List<string> transactionHistory = new List<string>();

        static void Main(string[] args)
        {
            while (true)
            {
                // 1. تشغيل دالة تسجيل الدخول
                bool isSuccess = PerformLogin();

                // 2. إذا نجح الدخول يتم الدخول إلى برنامج الصراف
                if (isSuccess)
                {
                    bool running = true;

                    while (running)
                    {
                        Console.Clear();
                        Console.WriteLine("========================================");
                        Console.WriteLine("           WELCOME TO MY BANK           ");
                        Console.WriteLine("========================================");
                        Console.WriteLine($"Balance: {balance:F2} JOD");
                        Console.WriteLine("----------------------------------------");
                        Console.WriteLine("1. Check Balance");
                        Console.WriteLine("2. Deposit Money");
                        Console.WriteLine("3. Withdraw Money");
                        Console.WriteLine("4. Transfer Money");
                        Console.WriteLine("5. Change PIN");
                        Console.WriteLine("6. Transaction History");
                        Console.WriteLine("7. Mini Statement");
                        Console.WriteLine("8. Logout");
                        Console.WriteLine("9. Exit");
                        Console.WriteLine("----------------------------------------");
                        Console.Write("Please select an option: ");

                        string choice = Console.ReadLine();

                        switch (choice)
                        {
                            case "1":
                                CheckBalance();
                                break;
                            case "2":
                                Deposit();
                                break;
                            case "3":
                                withdraw();
                                break;
                            case "4":
                                transferMoney();
                                break;
                            case "5":
                                changePIN();
                                break;
                            case "6":
                                ShowTransactionHistory();
                                break;
                            case "7":
                                ShowMiniStatement();
                                break;
                            case "8":
                                running = false;
                                Console.WriteLine("\nLogging out... Returning to login screen.");
                                Console.WriteLine("Press any key to continue...");
                                Console.ReadKey();
                                break;
                            case "9":
                                running = false;
                                Console.WriteLine("\nThank you for using our ATM. Goodbye!");
                                return;
                            default:
                                Console.WriteLine("\nInvalid option. Please try again.");
                                break;
                        }

                        if (running)
                        {
                            Console.WriteLine("\nPress any key to continue...");
                            Console.ReadKey();
                        }
                    }
                }
                else
                {
                    Console.WriteLine("\nProgram Terminated.");
                    return;
                }
            }
        }

        // دالة إدخال مبالغ مع حماية كاملة من الإدخال الخاطئ وإعادة التكرار
        static decimal ReadValidAmount(string prompt)
        {
            decimal amount;
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine();

                if (decimal.TryParse(input, out amount) && amount > 0)
                {
                    return amount;
                }
                Console.WriteLine("\nInvalid amount. Please enter a valid number.\n");
            }
        }

        // دالة لقراءة النصوص وتعديلها لمنع الإدخال الفارغ
        static string ReadNonEmptyString(string prompt, string errorMessage)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(input))
                {
                    return input;
                }
                Console.WriteLine(errorMessage);
            }
        }

        // 1. دالة قراءة الـ PIN مع الإخفاء
        static string ReadMaskedPassword()
        {
            string pass = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true); // قراءة المفتاح بدون إظهاره

                if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Backspace)
                {
                    pass += key.KeyChar;
                    Console.Write("*"); // طباعة النجمة بدلاً من الرقم
                }
                else if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    pass = pass.Substring(0, pass.Length - 1);
                    Console.Write("\b \b"); // مسح الرمز الأخير من الشاشة
                }
            }
            while (key.Key != ConsoleKey.Enter);

            Console.WriteLine(); // سطر جديد بعد ضغط Enter
            return pass;
        }

        static bool ValidatePin(string inputPin)
        {
            return inputPin == correctPin;
        }

        // 2. دالة تسجيل الدخول والمحاولات
        static bool PerformLogin()
        {
            if (!IsAtmOpen())
            {
                return false;
            }

            int attempts = 0;
            const int maxAttempts = 3;

            while (attempts < maxAttempts)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("                ATM LOGIN                ");
                Console.WriteLine("========================================");

                // تم ربطها بدالة ReadNonEmptyString لمنع ترك النص فارغاً
                string inputAccount = ReadNonEmptyString("Enter Account Number: ", "\nAccount number cannot be empty.\n");

                Console.Write("Enter PIN: ");
                string inputPin = ReadMaskedPassword();

                if (string.IsNullOrWhiteSpace(inputPin))
                {
                    Console.WriteLine("\nPIN cannot be empty.");
                    Console.WriteLine("Press any key to try again...");
                    Console.ReadKey();
                    continue;
                }

                // فحص صحة البيانات
                if (inputAccount == correctAccount && ValidatePin(inputPin))
                {
                    Console.WriteLine("\nLogin Successful!");
                    return true;
                }

                attempts++;
                int remaining = maxAttempts - attempts;

                if (remaining > 0)
                {
                    Console.WriteLine($"\nIncorrect credentials. Remaining attempts: {remaining}");
                    Console.WriteLine("Press any key to try again...");
                    Console.ReadKey();
                }
            }

            // بعد 3 محاولات خاطئة
            Console.WriteLine("\nAccount locked! You have exceeded the maximum attempts.");
            return false;
        }

        // 3. دالة عرض الرصيد
        static void CheckBalance()
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("             CURRENT BALANCE             ");
            Console.WriteLine("========================================");
            Console.WriteLine($"Current Balance: {balance:F2} JOD");
            Console.WriteLine("========================================");
        }

        // 4. دالة الإيداع النقدي
        static void Deposit()
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("              DEPOSIT MONEY              ");
            Console.WriteLine("========================================");

            // تم التعديل لاستخدام ReadValidAmount
            decimal amount = ReadValidAmount("Enter deposit amount: ");

            balance += amount;

            Console.WriteLine("\nDeposit successful!");
            Console.WriteLine($"New Balance: {balance:F2} JOD");

            string record = $"{transactionHistory.Count + 1}. Deposit\t\t+{amount:F2} JOD\t({DateTime.Now:yyyy-MM-dd HH:mm})";
            transactionHistory.Add(record);

            totalDeposited += amount;
        }

        // 5. دالة السحب النقدي
        static void withdraw()
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("              Withdraw Money             ");
            Console.WriteLine("========================================");
            Console.WriteLine($"Available Balance: {balance:F2} JOD");
            Console.WriteLine($"Daily Withdrawal Limit: {DAILY_WITHDRAWAL_LIMIT:F2} JOD");
            Console.WriteLine($"Withdrawn Today: {withdrawnToday:F2} JOD");
            Console.WriteLine("----------------------------------------");

            // تم التعديل لاستخدام ReadValidAmount
            decimal amount = ReadValidAmount("Enter withdrawal amount: ");

            if (amount > balance)
            {
                Console.WriteLine("\nInsufficient balance in your account!");
            }
            else if (withdrawnToday + amount > DAILY_WITHDRAWAL_LIMIT)
            {
                decimal remainingDailyLimit = DAILY_WITHDRAWAL_LIMIT - withdrawnToday;
                Console.WriteLine($"\nWithdrawal exceeds daily limit!");
                Console.WriteLine($"Remaining Daily Limit: {remainingDailyLimit:F2} JOD");
            }
            else
            {
                balance -= amount;
                withdrawnToday += amount;
                decimal remainingDailyLimit = DAILY_WITHDRAWAL_LIMIT - withdrawnToday;

                Console.WriteLine("\nPlease take your cash.");
                Console.WriteLine($"Remaining Balance: {balance:F2} JOD");
                Console.WriteLine($"Remaining Daily Limit: {remainingDailyLimit:F2} JOD");

                string record = $"{transactionHistory.Count + 1}. Withdrawal\t\t-{amount:F2} JOD\t({DateTime.Now:yyyy-MM-dd HH:mm})";
                transactionHistory.Add(record);

                totalWithdrawn += amount;
            }
        }

        // 6. دالة تحويل الأموال
        static void transferMoney()
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("              TRANSFER MONEY             ");
            Console.WriteLine("========================================");
            Console.WriteLine($"Available Balance: {balance:F2} JOD");
            Console.WriteLine("----------------------------------------");

            string targetAccount = ReadNonEmptyString("Enter destination account: ", "\nInvalid account number.\n");

            if (targetAccount == correctAccount)
            {
                Console.WriteLine("\nCannot transfer to your own account!");
                return;
            }

            // تم التعديل لاستخدام ReadValidAmount
            decimal amount = ReadValidAmount("Enter transfer amount: ");

            if (amount > balance)
            {
                Console.WriteLine("\nInsufficient balance.");
                return;
            }

            balance -= amount;
            Console.WriteLine("\nTransfer successful!");
            Console.WriteLine($"{amount:F2} JOD has been transferred to account {targetAccount}");
            Console.WriteLine($"Remaining Balance: {balance:F2} JOD");

            string record = $"{transactionHistory.Count + 1}. Transfer\t\t-{amount:F2} JOD\t({DateTime.Now:yyyy-MM-dd HH:mm})";
            transactionHistory.Add(record);

            totalTransferred += amount;
        }

        // 7. دالة تغيير PIN
        static void changePIN()
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("                 Change PIN              ");
            Console.WriteLine("========================================");

            Console.Write("Enter current PIN: ");
            string InputcurrentPin = ReadMaskedPassword();

            if (!ValidatePin(InputcurrentPin))
            {
                Console.WriteLine("\nIncorrect current PIN!");
                return;
            }

            Console.Write("Enter new PIN: ");
            string newPin = ReadMaskedPassword();

            Console.Write("Confirm new PIN: "); // تم تعديلها إلى Write بدلاً من WriteLine
            string confirmPin = ReadMaskedPassword();

            if (newPin != confirmPin)
            {
                Console.WriteLine("\nNew PIN and confirmation do not match!");
                return;
            }
            if (newPin.Length != 4 || !newPin.All(char.IsDigit))
            {
                Console.WriteLine("\nPIN must be exactly 4 digits!");
                return;
            }
            if (ValidatePin(newPin))
            {
                Console.WriteLine("\nNew PIN must be different from current PIN!");
                return;
            }

            correctPin = newPin;
            Console.WriteLine("\nPIN changed successfully!");
        }

        // 8. دالة سجل العمليات
        static void ShowTransactionHistory()
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("           TRANSACTION HISTORY           ");
            Console.WriteLine("========================================");

            if (transactionHistory.Count == 0)
            {
                Console.WriteLine("No transaction performed yet.");
            }
            else
            {
                foreach (string transaction in transactionHistory)
                {
                    Console.WriteLine(transaction);
                }
            }
            Console.WriteLine("----------------------------------------");
        }

        // 9. دالة الكشف المختصر
        static void ShowMiniStatement()
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("              MINI STATEMENT             ");
            Console.WriteLine("========================================");
            Console.WriteLine($"Account Number: {correctAccount}");
            Console.WriteLine($"Current Balance: {balance:F2} JOD");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"Total Deposited: {totalDeposited:F2} JOD");
            Console.WriteLine($"Total Withdrawn: {totalWithdrawn:F2} JOD");
            Console.WriteLine($"Total Transferred: {totalTransferred:F2} JOD");
            Console.WriteLine("========================================");
        }

        // 10. دالة التحقق من ساعات العمل
        static bool IsAtmOpen()
        {
            TimeSpan currentTime = DateTime.Now.TimeOfDay;

            TimeSpan startTime = new TimeSpan(6, 0, 0); // 06:00 AM
            TimeSpan endTime = new TimeSpan(23, 0, 0); // 11:00 PM

            if (currentTime >= startTime && currentTime <= endTime)
            {
                return true;
            }
            else
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("      ATM IS CURRENTLY UNAVAILABLE      ");
                Console.WriteLine("========================================");
                Console.WriteLine("Operating Hours: 06:00 AM - 11:00 PM");
                Console.WriteLine("Please try again during working hours.");
                Console.WriteLine("========================================");
                return false;
            }
        }
    }
}