﻿using System;
using Cable.Modules;

namespace Cable
{
    internal class Program
    {

        public static void Help()
        {
            string help =
                "Cable.exe [Module]\n" +
                "Modules:\n" +
                "\tenum [Options]       - Enumerate LDAP\n" +
                "\tkerberoast <account> - Kerberoast a potentially supplied account, or everything\n" +
                "\tdclist               - List Domain Controllers in the current Domain\n" +
                "\trbcd [Options]       - Write or remove the msDs-AllowedToActOnBehalfOfOtherIdentity attribute\n" +
                "\ttrusts               - Enumerate Active Directory Domain Trusts in the current Forest\n" +
                "\ttemplates            - Enumerate Active Directory Certificate Services (ADCS) Templates\n\n" +

                "Module Options\n" +
                "enum:\n" +
                "\t--users          - Enumerate user objects\n" +
                "\t--computers      - Enumerate computer objects\n" +
                "\t--groups         - Enumerate group objects\n" +
                "\t--gpos           - Enumerate Group Policy objects\n" +
                "\t--spns           - Enumerate objects with servicePrincipalName set\n" +
                "\t--asrep          - Enumerate accounts that do not require Kerberos pre-authentication\n" +
                "\t--admins         - Enumerate accounts with adminCount set to 1\n" +
                "\t--constrained    - Enumerate accounts with msDs-AllowedToDelegateTo set\n" +
                "\t--unconstrained  - Enumerate accounts with the TRUSTED_FOR_DELEGATION flag set\n" +
                "\t--rbcd           - Enumerate accounts with msDs-AllowedToActOnBehalfOfOtherIdentity set\n" +
                "\t--filter <query> - Enumerate objects with a custom set query\n\n" +

                "rbcd:\n" +
                "\t--write                   - Operation to write msDs-AllowedToActOnBehalfOfOtherIdentity\n" +
                "\t--delegate-to <account>   - Target account to delegate access to\n" +
                "\t--delegate-from <account> - Controlled account to delegate from\n" +
                "\t--flush <account>         - Operation to flush msDs-AllowedToActOnBehalfOfOtherIdentity on an account\n";

            Console.WriteLine(help);

        }

        static void Main(string[] args)
        {

            Console.WriteLine(" ________  ________  ________  ___       _______      \r\n|\\   ____\\|\\   __  \\|\\   __  \\|\\  \\     |\\  ___ \\     \r\n\\ \\  \\___|\\ \\  \\|\\  \\ \\  \\|\\ /\\ \\  \\    \\ \\   __/|    \r\n \\ \\  \\    \\ \\   __  \\ \\   __  \\ \\  \\    \\ \\  \\_|/__  \r\n  \\ \\  \\____\\ \\  \\ \\  \\ \\  \\|\\  \\ \\  \\____\\ \\  \\_|\\ \\ \r\n   \\ \\_______\\ \\__\\ \\__\\ \\_______\\ \\_______\\ \\_______\\\r\n    \\|_______|\\|__|\\|__|\\|_______|\\|_______|\\|_______|\r\n");

            try
            {
                if (args.Length > 0)
                {
                    if (args[0] == "kerberoast")
                    {
                        if (args.Length > 1)
                        {
                            string[] spn = { args[1] };
                            Kerberoast.Roast(spn);
                        }
                        else
                        {
                            string[] spns = Kerberoast.GetSPNs();
                            Kerberoast.Roast(spns);
                        }

                    }

                    else if (args[0] == "templates")
                    {
                        ADCS.templateLookup();
                    }

                    else if (args[0] == "rbcd")
                    {
                        string delegate_from = "";
                        string delegate_to = "";
                        string operation = "";
                        string account = "";

                        for (int i = 0; i < args.Length; i++)
                        {
                            switch (args[i])
                            {
                                case "--delegate-to":
                                    delegate_to = args[i + 1];
                                    break;
                                case "--delegate-from":
                                    delegate_from = args[i + 1];
                                    break;
                                case "--write":
                                    operation = "write";
                                    break;
                                case "--flush":
                                    operation = "flush";
                                    if (delegate_to == "" && delegate_from == "")
                                    {
                                        if (args.Length > 2)
                                        {
                                            account = args[i + 1];
                                        }
                                        else
                                        {
                                            Console.WriteLine("[!] Error: please supply an account to flush");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("[!] Error: supplied delegate_from or delegate_to with --flush option");
                                        return;
                                    }
                                    break;
                            }
                        }
                                                
                        if (operation == "write")
                        {
                            if (delegate_from == "" || delegate_to == "" || operation == "")
                            {
                                Console.WriteLine("[!] You must specify all the parameters required for an RBCD write\n ");
                                Help();
                            }
                            else
                            {
                                RBCD.WriteRBCD(delegate_to, delegate_from);
                            }
                        }
                        else if (operation == "flush"){
                            if (account == "" || operation == "")
                            {
                                Console.WriteLine("[!] You must specify all the parameters required for an RBCD flush\n ");
                                Help();
                            }
                            else
                            {
                                RBCD.FlushRBCD(account);
                            }
                        }
                        else
                        {
                            Help();
                        }
                        
                    }

                    else if (args[0] == "enum")
                    {
                        if (args.Length > 1)
                        {
                            Enumerate.Enum(args[1], args);
                        }
                        else
                        {
                            Help();
                        }
                    }

                    else if (args[0] == "dclist")
                    {
                        Enumerate.Dclist();
                    }

                    else if (args[0] == "trusts")
                    {
                        Enumerate.enumTrusts();
                    }

                    else
                    {
                        Console.WriteLine("[!] Command not recognized\n");
                        Help();
                    }
                }
                else
                {
                    Help();
                }
            }


            catch (Exception e)
            {
                Console.WriteLine("[!] Exception: " + e.ToString());
            }

        }
    }
}
