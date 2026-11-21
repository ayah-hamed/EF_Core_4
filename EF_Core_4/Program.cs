using System.Globalization;
using System.Net.Mail;
using EF_Core_4.Data;
using EF_Core_4.Entities;
using EF_Core_4.Enums;
using Microsoft.EntityFrameworkCore;

namespace EF_Core_4;

internal class Program
{
    #region Main and menu

    static void Main(string[] args)
    {
        while (true)
        {
            Console.Clear();
            ShowMenu();

            int choice = ReadInt("Enter choice: ", 0, 5);

            if (choice == 0)
            {
                Console.WriteLine("Goodbye!");
                return;
            }

            Console.Clear();

            try
            {
                switch (choice)
                {
                    case 1:
                        AddCustomer();
                        break;

                    case 2:
                        OpenAccount();
                        break;

                    case 3:
                        UpdateAccountStatus();
                        break;

                    case 4:
                        RemoveCustomerAccount();
                        break;

                    case 5:
                        ListCustomers();
                        break;
                }
            }
            catch (DbUpdateException)
            {
                // show a friendly message if the database rejects the change
                Console.WriteLine(
                    "The change couldn't be saved. Check for duplicate " +
                    "values or related records.");
            }
            catch (Microsoft.Data.SqlClient.SqlException)
            {
                Console.WriteLine(
                    "The database operation failed. Check your connection " +
                    "and make sure the database is available.");
            }

            Pause();
        }
    }

    static void ShowMenu()
    {
        Console.WriteLine("==================================================");
        Console.WriteLine("          National Bank - Management");
        Console.WriteLine("==================================================");
        Console.WriteLine("1) Add a new Customer");
        Console.WriteLine("2) Open a new Account for a Customer");
        Console.WriteLine("3) Update Account Status (Active / Closed)");
        Console.WriteLine("4) Remove an Account from a Customer");
        Console.WriteLine("5) List all Customers (with accounts)");
        Console.WriteLine("0) Exit");
        Console.WriteLine("--------------------------------------------------");
    }

    static void Pause()
    {
        Console.WriteLine();
        Console.WriteLine("Press any key to return to the menu...");
        Console.ReadKey(true);
    }

    #endregion

    #region Input helpers

    static int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);

            // ask again when the input isn't a number in the allowed range
            if (int.TryParse(Console.ReadLine(), out int value)
                && value >= min
                && value <= max)
            {
                return value;
            }

            Console.WriteLine(
                $"Please enter a whole number between {min} and {max}.");
        }
    }

    static string ReadRequired(string prompt, int maxLength)
    {
        while (true)
        {
            Console.Write(prompt);
            string value = Console.ReadLine()?.Trim() ?? "";

            if (!string.IsNullOrWhiteSpace(value)
                && value.Length <= maxLength)
            {
                return value;
            }

            Console.WriteLine(
                $"Please enter a value between 1 and {maxLength} characters.");
        }
    }

    #endregion

    #region 1 - Add customer

    static void AddCustomer()
    {
        Console.WriteLine("--- Add New Customer ---");

        // use a fresh context for this operation
        using var context = new AppDbContext();

        // ReadRequired keeps asking until the value isn't empty or too long
        string fullName = ReadRequired("Full Name: ", 100);

        string nationalId;

        while (true)
        {
            nationalId = ReadRequired("National ID: ", 30);

            // keep the id as text so leading zeros aren't lost
            if (!nationalId.All(c => c >= '0' && c <= '9'))
            {
                Console.WriteLine("National ID must contain digits only.");
                continue;
            }

            // check for an existing customer before trying to save a duplicate
            if (context.Customers.Any(c => c.NationalId == nationalId))
            {
                Console.WriteLine("A customer with this national ID already exists.");
                continue;
            }

            break;
        }

        DateTime dateOfBirth;

        while (true)
        {
            Console.Write("Date of Birth (yyyy-MM-dd): ");

            // require the displayed date format and reject today or future dates
            if (DateTime.TryParseExact(
                    Console.ReadLine()?.Trim(),
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out dateOfBirth)
                && dateOfBirth.Date < DateTime.Today)
            {
                break;
            }

            Console.WriteLine("Please enter a valid date before today.");
        }

        string email;

        while (true)
        {
            email = ReadRequired("Email: ", 150);

            // accept a plain email address without a display name
            if (MailAddress.TryCreate(email, out var parsedEmail)
                && parsedEmail.Address == email)
            {
                break;
            }

            Console.WriteLine("Please enter a valid email address.");
        }

        string phone;

        while (true)
        {
            phone = ReadRequired("Phone: ", 20);

            // ignore the optional + while checking the remaining characters
            string digits = phone.StartsWith("+") ? phone[1..] : phone;

            if (digits.Length >= 7
                && digits.All(c => c >= '0' && c <= '9'))
            {
                break;
            }

            Console.WriteLine(
                "Enter at least 7 digits, optionally starting with +.");
        }

        string address = ReadRequired("Address: ", 250);

        Console.WriteLine("Customer Type:");
        Console.WriteLine("1) Individual");
        Console.WriteLine("2) Business");

        // the menu numbers match the values defined in CustomerType
        var customerType = (CustomerType)ReadInt("Choice: ", 1, 2);

        // create the customer only after all inputs have passed validation
        var customer = new Customer
        {
            FullName = fullName,
            NationalId = nationalId,
            DateOfBirth = dateOfBirth,
            Email = email,
            PhoneNumber = phone,
            Address = address,
            CustomerType = customerType
        };

        context.Customers.Add(customer);
        context.SaveChanges();

        // the generated id is available after saving
        Console.WriteLine();
        Console.WriteLine(
            $"Customer created successfully. CustomerId = {customer.Id}");
    }

    #endregion

    #region 2 - Open account

    static void OpenAccount()
    {
        Console.WriteLine("--- Open New Account ---");

        using var context = new AppDbContext();

        string accountNumber;

        while (true)
        {
            accountNumber = ReadRequired("Account Number: ", 30);

            // this option creates a new account, so its number must be unused
            if (context.Accounts.Any(a => a.AccountNumber == accountNumber))
            {
                Console.WriteLine(
                    "This account number already exists. Enter a different number.");
                continue;
            }

            break;
        }

        Console.WriteLine("Account Type:");
        Console.WriteLine("1) Savings");
        Console.WriteLine("2) Current");
        Console.WriteLine("3) Business");

        var accountType = (AccountType)ReadInt("Choice: ", 1, 3);

        string branchCode = ReadRequired("Branch Code: ", 20);
        int customerId = ReadInt("Customer Id: ", 1, int.MaxValue);

        Console.WriteLine("Ownership Role:");
        Console.WriteLine("1) Primary");
        Console.WriteLine("2) CoHolder");

        var ownershipType = (OwnershipType)ReadInt("Choice: ", 1, 2);

        Console.WriteLine();
        Console.WriteLine(
            $"Validating branch '{branchCode}' and customer #{customerId}...");

        // an account must belong to a branch that already exists
        var branch = context.Branches.Find(branchCode);

        if (branch == null)
        {
            Console.WriteLine("Branch not found. No account was created.");
            return;
        }

        // the ownership link also needs an existing customer
        var customer = context.Customers.Find(customerId);

        if (customer == null)
        {
            Console.WriteLine("Customer not found. No account was created.");
            return;
        }

        // new accounts start with a zero balance
        var account = new Account
        {
            AccountNumber = accountNumber,
            AccountType = accountType,
            CurrentBalance = 0m,
            OpeningDate = DateTime.Today,
            BranchCode = branch.Code
        };

        // ownership details belong to the link, not to the account itself
        var ownership = new CustomerAccount
        {
            CustomerId = customer.Id,
            AccountNumber = account.AccountNumber,
            OwnershipType = ownershipType,
            OwnershipStartDate = DateTime.Today,
            AccountStatus = AccountStatus.Active,
            Account = account
        };

        // save the account and ownership link together
        context.Accounts.Add(account);
        context.CustomerAccounts.Add(ownership);
        context.SaveChanges();

        Console.WriteLine(
            $"Account '{account.AccountNumber}' created and linked to " +
            $"customer {customer.Id} as {ownershipType} owner.");
    }

    #endregion

    #region 3 - Update account status

    static void UpdateAccountStatus()
    {
        Console.WriteLine("--- Update Account Status ---");

        string accountNumber = ReadRequired("Account Number: ", 30);
        int customerId = ReadInt("Customer Id: ", 1, int.MaxValue);

        using var context = new AppDbContext();

        // use both key values to find this customer's ownership link
        // the order must match the composite key: CustomerId, AccountNumber
        var ownership = context.CustomerAccounts.Find(
            customerId, accountNumber);

        if (ownership == null)
        {
            Console.WriteLine(
                "No ownership link was found for this customer and account.");
            return;
        }

        Console.WriteLine($"Current Status: {ownership.AccountStatus}");
        Console.WriteLine("New Status:");
        Console.WriteLine("1) Active");
        Console.WriteLine("2) Closed");

        var newStatus = (AccountStatus)ReadInt("Choice: ", 1, 2);

        // don't save when the selected status is already set
        if (ownership.AccountStatus == newStatus)
        {
            Console.WriteLine($"Status is already {newStatus}.");
            return;
        }

        // change the status on this ownership link only
        ownership.AccountStatus = newStatus;
        context.SaveChanges();

        Console.WriteLine($"Status updated to {newStatus}.");
    }

    #endregion

    #region 4 - Remove account from customer

    static void RemoveCustomerAccount()
    {
        Console.WriteLine("--- Remove Account From Customer ---");

        string accountNumber = ReadRequired("Account Number: ", 30);
        int customerId = ReadInt("Customer Id: ", 1, int.MaxValue);

        using var context = new AppDbContext();

        // load the account with the link in case we need to delete both
        var ownership = context.CustomerAccounts
            .Include(ca => ca.Account)
            .FirstOrDefault(ca =>
                ca.CustomerId == customerId
                && ca.AccountNumber == accountNumber);

        if (ownership == null)
        {
            Console.WriteLine(
                "No ownership link was found for this customer and account.");
            return;
        }

        // another owner means the account must stay after this link is removed
        bool hasOtherOwners = context.CustomerAccounts.Any(ca =>
            ca.AccountNumber == accountNumber
            && ca.CustomerId != customerId);

        if (!hasOtherOwners)
        {
            bool hasTransactions = context.Transactions.Any(t =>
                t.AccountNumber == accountNumber);

            // protect transaction history instead of deleting it with the account
            // leave both the account and its last ownership link unchanged
            if (hasTransactions)
            {
                Console.WriteLine(
                    "Cannot remove the last owner because this account has " +
                    "transaction history. No changes were made.");
                return;
            }
        }

        // remove the relationship, not the customer
        context.CustomerAccounts.Remove(ownership);

        if (!hasOtherOwners)
        {
            // the sample output also removes the account when its last owner leaves
            context.Accounts.Remove(ownership.Account);
        }

        // save both deletions together rather than leaving a partially saved change
        context.SaveChanges();

        Console.WriteLine("Ownership link deleted.");

        if (!hasOtherOwners)
        {
            Console.WriteLine(
                $"That was the last owner - account '{accountNumber}' " +
                "was also removed.");
        }
        else
        {
            Console.WriteLine(
                "The account was kept because it still has other owners.");
        }
    }

    #endregion

    #region 5 - List customers with accounts

    static void ListCustomers()
    {
        Console.WriteLine("--- All Customers ---");
        Console.WriteLine();

        using var context = new AppDbContext();

        // eagerly load the full path needed for the printed list
        // no tracking is needed because this operation doesn't change any data
        var customers = context.Customers
            .AsNoTracking()
            .Include(c => c.CustomerAccounts)
                .ThenInclude(ca => ca.Account)
                    .ThenInclude(a => a.Branch)
            .OrderBy(c => c.Id)
            .ToList();

        if (customers.Count == 0)
        {
            Console.WriteLine("No customers found.");
            return;
        }

        foreach (var customer in customers)
        {
            Console.WriteLine(
                $"#{customer.Id} {customer.FullName} ({customer.CustomerType})");

            // keep customers visible even when they don't own any accounts
            if (customer.CustomerAccounts.Count == 0)
            {
                Console.WriteLine("    (no accounts)");
            }
            else
            {
                foreach (var ownership in customer.CustomerAccounts
                             .OrderBy(ca => ca.AccountNumber))
                {
                    var account = ownership.Account;

                    // align the columns to keep multiple accounts easy to read
                    Console.WriteLine(
                        $"    {account.AccountNumber,-15} " +
                        $"{account.AccountType,-9} " +
                        $"Balance: {account.CurrentBalance,14:N2}  " +
                        $"{ownership.OwnershipType,-8} " +
                        $"{ownership.AccountStatus,-6} " +
                        $"@ {account.Branch.Name}");
                }
            }

            Console.WriteLine();
        }
    }

    #endregion


}