using Microsoft.Extensions.Logging;
using System;

namespace SodaMachine
{
    public interface IUserInterfaceController
    {
        int ProcessTransactionWithCash();
        int ProcessOrderWithSMS();
    }
    public class UserInterfaceController : IUserInterfaceController
    {
        private readonly ILogger<UserInterfaceController> _logger;
        private readonly SodaMachineDbContext _sodaMachineDbContext;
        private readonly ISodaMachineService _sodaMachineService;
        private readonly Guid customerId = Guid.NewGuid();

        public UserInterfaceController(
            ISodaMachineService sodaMachineService,
            SodaMachineDbContext sodaMachineDbContext,
            ILogger<UserInterfaceController> logger
            )
        {

            _sodaMachineDbContext = sodaMachineDbContext ?? throw new ArgumentNullException(nameof(sodaMachineDbContext));
            _sodaMachineService = sodaMachineService ?? throw new ArgumentNullException(nameof(sodaMachineService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public int ProcessTransactionWithCash()
        {

            Console.WriteLine("Press i to insert money");
            char userInputSelectionForMoney = char.MinValue;
            var moneyInput = string.Empty;
            double money = 0;
            var userInputForSodaSelection = char.MinValue;
            var quitInput = char.MinValue;
            do
            {
                #region input and validation

                Console.WriteLine();
                if (userInputSelectionForMoney == char.MinValue)
                {
                    quitInput = userInputSelectionForMoney = Console.ReadKey().KeyChar;
                    Console.WriteLine();
                    if (quitInput.ToString().ToLower() == "q")
                    {
                        Recall();
                        break;
                    }
                    if (userInputSelectionForMoney.ToString().ToLower() != "i")
                    {
                        _logger.LogError($"Please press i instead of {userInputSelectionForMoney.ToString()}");
                        userInputSelectionForMoney = char.MinValue;
                        continue;
                    }
                }
                if (string.IsNullOrEmpty(moneyInput))
                {
                    Console.WriteLine();
                    Console.WriteLine("Please enter the amount");
                    moneyInput = Console.ReadLine();
                    if (string.IsNullOrEmpty(moneyInput))
                    {
                        Console.WriteLine("Please press q if you choose to recall");
                        quitInput = Console.ReadKey().KeyChar;
                        continue;
                    }
                    double.TryParse(moneyInput, out money);
                    if (money <= 0)
                    {
                        _logger.LogError($"Please provide money which more than zero instead of {moneyInput}");
                        moneyInput = string.Empty;
                        continue;
                    }
                    _sodaMachineService.AddCustomer(customerId, money);
                    Console.WriteLine($"You provided amount is {money}/- ");
                }

                Console.WriteLine();
                if (userInputForSodaSelection == char.MinValue)
                {
                    Console.WriteLine("Press 1 to select Coke");
                    Console.WriteLine("Press 2 to select Fanta");
                    Console.WriteLine("Press 3 to select Sprite");
                    quitInput = userInputForSodaSelection = Console.ReadKey().KeyChar;
                    Console.WriteLine();
                    if (quitInput.ToString().ToLower() == "q")
                    {
                        Recall();
                        break;
                    }
                    if (userInputForSodaSelection.ToString() != "1" && userInputForSodaSelection.ToString() != "2" && userInputForSodaSelection.ToString() != "3")
                    {
                        _logger.LogError($"Please press 1 (Coke), 2 (Fanta) or 3 (Sprite) instead of {userInputForSodaSelection.ToString()}");
                        userInputForSodaSelection = char.MinValue;
                        continue;
                    }
                    Console.WriteLine();
                }

                var selectedSodaName = GetSelectedSodaName(userInputForSodaSelection.ToString());
                var selectedSoda = _sodaMachineService.GetSelectedSoda(selectedSodaName);
                Console.WriteLine($"You chose {selectedSodaName} with current price {selectedSoda.Price}");
                var currentCustomerBalance = _sodaMachineService.GetCurrentCustomerBalance(customerId).GetAwaiter().GetResult();
                if (currentCustomerBalance < selectedSoda.Price)
                {
                    userInputSelectionForMoney = char.MinValue;
                    _logger.LogError($"The amount {currentCustomerBalance}/- you have is not enough to buy {selectedSodaName}. Please try with different amount");
                    continue;
                }
                if (selectedSoda.Quantity < 0)
                {
                    _logger.LogError($"The {selectedSodaName} is out of stock. Please try sometime later.");
                    break;
                }

                #endregion input and validation

                #region processing

                _sodaMachineService.AddSelectedItemToCustomerSelection(customerId, selectedSoda).GetAwaiter().GetResult();

                _sodaMachineService.AdjustCustomerBalance(customerId).GetAwaiter().GetResult();

                _sodaMachineService.AdjustInventory(customerId, selectedSoda).GetAwaiter().GetResult();

                #endregion processing

                currentCustomerBalance = _sodaMachineService.GetCurrentCustomerBalance(customerId).GetAwaiter().GetResult();
                Console.WriteLine($"Your balance is {currentCustomerBalance}/-");

                if (currentCustomerBalance <= 0)
                {
                    break;
                }
                selectedSoda = _sodaMachineService.GetSelectedSoda(selectedSodaName);
                if (selectedSoda.Quantity <= 0)
                {
                    _logger.LogError("Out of stock");
                    break;
                }

                Console.WriteLine($"To buy another soda press y, or press q to quit");
                var toContinue = char.MinValue;
                quitInput = toContinue = Console.ReadKey().KeyChar;
                if (quitInput.ToString().ToLower() == "q")
                {
                    Recall();
                    break;
                }
                if (toContinue.ToString().ToLower() == "y")
                {
                    userInputForSodaSelection = char.MinValue;
                    continue;
                }

                Console.WriteLine("Please press q if you choose to recall");
                quitInput = Console.ReadKey().KeyChar;

            } while (quitInput.ToString().ToLower() != "q");

            return 1;
        }

        public int ProcessOrderWithSMS()
        {
            var quitInput = char.MinValue;
            var userInputForSodaSelection = char.MinValue;
            do
            {
                Console.WriteLine();
                if (userInputForSodaSelection == char.MinValue)
                {
                    _sodaMachineService.AddCustomer(customerId);
                    Console.WriteLine("Press 1 to select Coke");
                    Console.WriteLine("Press 2 to select Fanta");
                    Console.WriteLine("Press 3 to select Sprite");
                    quitInput = userInputForSodaSelection = Console.ReadKey().KeyChar;
                    Console.WriteLine();
                    if (quitInput.ToString().ToLower() == "q")
                    {
                        Console.WriteLine("Quiting...");
                        break;
                    }
                    if (userInputForSodaSelection.ToString() != "1" && userInputForSodaSelection.ToString() != "2" && userInputForSodaSelection.ToString() != "3")
                    {
                        _logger.LogError($"Please press 1 (Coke), 2 (Fanta) or 3 (Sprite) instead of {userInputForSodaSelection.ToString()}");
                        userInputForSodaSelection = char.MinValue;
                        continue;
                    }
                    Console.WriteLine();
                }

                var selectedSodaName = GetSelectedSodaName(userInputForSodaSelection.ToString());
                var selectedSoda = _sodaMachineService.GetSelectedSoda(selectedSodaName);
                Console.WriteLine($"You chose {selectedSodaName} remaining quantity {selectedSoda.Quantity}");
                if (selectedSoda.Quantity < 0)
                {
                    _logger.LogError($"The {selectedSodaName} is out of stock. Please try sometime later.");
                    break;
                }

                _sodaMachineService.AddSelectedItemToCustomerSelection(customerId, selectedSoda).GetAwaiter().GetResult();
                _sodaMachineService.AdjustInventory(customerId, selectedSoda).GetAwaiter().GetResult();

                Console.WriteLine($"To buy another soda press y, or press q to quit");
                var toContinue = char.MinValue;
                quitInput = toContinue = Console.ReadKey().KeyChar;
                if (quitInput.ToString().ToLower() == "q")
                {
                    break;
                }
                if (toContinue.ToString().ToLower() == "y")
                {
                    userInputForSodaSelection = char.MinValue;
                    continue;
                }
                
            } while (quitInput.ToString().ToLower() != "q");
            return 1;
        }

        private void Recall()
        {
            var currentBalance = _sodaMachineService.GetCurrentCustomerBalance(customerId).GetAwaiter().GetResult();
            Console.WriteLine();
            Console.WriteLine($"Returning Money {currentBalance}/-");
            Console.WriteLine("Thank you for using Soda Machine");
        }
        private string GetSelectedSodaName(string choice)
        {
            switch (choice)
            {
                case "1":
                    {
                        return NameOfSoda.Coke;
                    }
                case "2":
                    {
                        return NameOfSoda.Fanta;
                    }
                case "3":
                    {
                        return NameOfSoda.Sprite;
                    }
                default:
                    throw new Ardalis.GuardClauses.NotFoundException(choice, "No such soda found. Please try different choice");
            }
        }

        // end of class
    }
}
