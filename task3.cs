using System;

class PalindromeFinder
{
    static void Main()
    {
        Console.Write("Введите строку: ");
        string input = Console.ReadLine();
        
        string longest = "";
        int count = 0;
        
        for (int i = 0; i < input.Length; i++)
        {
            for (int j = i; j < input.Length; j++)
            {
                string sub = input.Substring(i, j - i + 1);
                
                bool isPalindrome = true;
                for (int k = 0; k < sub.Length / 2; k++)
                {
                    if (sub[k] != sub[sub.Length - 1 - k])
                    {
                        isPalindrome = false;
                        break;
                    }
                }
                
                if (isPalindrome)
                {
                    count++;
                    if (sub.Length > longest.Length)
                        longest = sub;
                }
            }
        }
        
        Console.WriteLine($"Самый длинный палиндром: {longest}");
        Console.WriteLine($"Всего палиндромов: {count}");
    }
}