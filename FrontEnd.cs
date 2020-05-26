using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

namespace SuncoastBank
{

    public class FrontEnd
    {

        private TextInfo Formatter = new CultureInfo("en-US", false).TextInfo;

        public int PromptFromList(string label, List<string> choices)
        {
            if (choices.Count() == 0)
            {
                Console.WriteLine("No matching choices!");
                return -1;
            }

            WriteList(label, choices);

            var userInput = PromptForInteger("choose", choices.Count);

            while (userInput == -1)
            {
                Console.WriteLine("Please make a valid selection!");
                userInput = PromptForInteger("choose", choices.Count);
            }
            Console.WriteLine();
            return userInput - 1;

        }

        // Front end function
        public int PromptForInteger(string label, int max = Int32.MaxValue)
        {
            WriteLabel(label);

            int userInput;
            var validInput = Int32.TryParse(Console.ReadLine(), out userInput);

            if (validInput && userInput <= max && userInput > 0)
            {
                return (int)userInput;
            }

            return -1;
        }

        // Front end function
        public string PromptForString(string label)
        {
            WriteLabel(label);
            return Console.ReadLine();
        }

        // Front end function
        public void WriteList(string label, List<string> choices)
        {
            int ordinal = 1;
            string formattedList = $"{Formatter.ToTitleCase(label)}";

            foreach (string choice in choices)
            {
                formattedList += $"\n\t({ordinal}) {choice}";
                ordinal++;
            }

            Console.WriteLine(formattedList);
        }

        // Front end function
        public void WriteLabel(string label)
        {
            Console.Write($"{Formatter.ToTitleCase(label)}: ");
        }

    }

}

