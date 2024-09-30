﻿using System;
using System.Collections.Generic;
using System.Linq;


namespace Cable.Modules
{
    internal class ArgParse
    {
        public static void Help()
        {
            string help =
                "Cable.exe [Module]\n" +
                "Modules:\n" +
                "\tenum [Options]            - Enumerate LDAP\n" +
                "\tkerberoast <account>      - Kerberoast a potentially supplied account, or everything\n" +
                "\tdclist                    - List Domain Controllers in the current Domain\n" +
                "\trbcd [Options]            - Write or remove the msDs-AllowedToActOnBehalfOfOtherIdentity attribute\n" +
                "\ttrusts                    - Enumerate Active Directory Domain Trusts in the current Forest\n" +
                "\ttemplates                 - Enumerate Active Directory Certificate Services (ADCS) Templates\n\n" +

                "Module Options\n" +
                "enum:\n" +
                "\t--users                   - Enumerate user objects\n" +
                "\t--computers               - Enumerate computer objects\n" +
                "\t--groups                  - Enumerate group objects\n" +
                "\t--gpos                    - Enumerate Group Policy objects\n" +
                "\t--spns                    - Enumerate objects with servicePrincipalName set\n" +
                "\t--asrep                   - Enumerate accounts that do not require Kerberos pre-authentication\n" +
                "\t--admins                  - Enumerate accounts with adminCount set to 1\n" +
                "\t--constrained             - Enumerate accounts with msDs-AllowedToDelegateTo set\n" +
                "\t--unconstrained           - Enumerate accounts with the TRUSTED_FOR_DELEGATION flag set\n" +
                "\t--rbcd                    - Enumerate accounts with msDs-AllowedToActOnBehalfOfOtherIdentity set\n" +
                "\t--query <query>           - Enumerate objects with a custom query\n\n" +

                "rbcd:\n" +
                "\t--write                   - Operation to write msDs-AllowedToActOnBehalfOfOtherIdentity\n" +
                "\t--delegate-to <account>   - Target account to delegate access to\n" +
                "\t--delegate-from <account> - Controlled account to delegate from\n" +
                "\t--flush <account>         - Operation to flush msDs-AllowedToActOnBehalfOfOtherIdentity on an account\n\n" +
                
                "group:\n" +
                "\t--getmembership           - Operation to get Active Directory group membership\n" +
                "\t--group <group>           - The group used for an operation specified\n" +
                "\t--add <account>           - Add a specified account to the group selected\n" +
                "\t--remove <account>        - Remove a specified account from the group selected\n";

            Console.WriteLine(help);

        }

        static List<string> sMods = new List<string> 
        { 
            "enum",
            "kerberoast",
            "dclist",
            "rbcd",
            "trusts",
            "templates",
            "group"
        };

        public static void Parse(string[] args)
        {
            if(args.Contains("--help") || args.Contains("-h") || args.Length == 0)
            {
                Help();
            }
            else if (sMods.Contains(args.First()))
            {
                try
                {
                    switch (args.First().ToLower())
                    {
                        case "enum":
                            if (args.Length > 1)
                            {
                                Enumerate.Enum(args[1], args);
                            }
                            else
                            {
                                Help();
                            }
                            break;
                        case "kerberoast":
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
                            break;
                        case "dclist":
                            Enumerate.Dclist();
                            break;
                        case "rbcd":
                            string delegate_from = "";
                            string delegate_to = "";
                            string rbcdoperation = "";
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
                                        rbcdoperation = "write";
                                        break;
                                    case "--flush":
                                        rbcdoperation = "flush";
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

                            if (rbcdoperation == "write")
                            {
                                if (delegate_from == "" || delegate_to == "" || rbcdoperation == "")
                                {
                                    Console.WriteLine("[!] You must specify all the parameters required for an RBCD write ");
                                }
                                else
                                {
                                    RBCD.WriteRBCD(delegate_to, delegate_from);
                                }
                            }
                            else if (rbcdoperation == "flush")
                            {
                                if (account == "" || rbcdoperation == "")
                                {
                                    Console.WriteLine("[!] You must specify all the parameters required for an RBCD flush");

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
                            break;
                        case "trusts":
                            Enumerate.enumTrusts();
                            break;
                        case "templates":
                            ADCS.templateLookup();
                            break;
                        case "group":
                            string group = "";
                            string groupoperation = "";
                            string user = "";
                            for (int i = 0; i < args.Length; i++)
                            {
                                switch (args[i])
                                {
                                    case "--getmembership":
                                        groupoperation = "getmem";
                                        break;
                                    case "--group":
                                        group = args[i + 1];
                                        break;
                                    case "--add":
                                        groupoperation = "add";
                                        user = args[i + 1];
                                        break;
                                    case "--remove":
                                        groupoperation = "remove";
                                        user = args[i + 1];
                                        break;
                                }
                            }
                            if(groupoperation == "add")
                            {
                                if (group == "")
                                {
                                    Console.WriteLine("[!] Please supply a group");
                                    return;
                                }
                                Groups.AddToGroup(user, group);
                            }
                            else if (groupoperation == "remove")
                            {
                                if (group == "")
                                {
                                    Console.WriteLine("[!] Please supply a group");
                                    return;
                                }
                                Groups.RemoveFromGroup(user, group);
                            }
                            else if (groupoperation == "getmem")
                            {
                                if(group == "")
                                {
                                    Console.WriteLine("[!] Please supply a group");
                                    return;
                                }
                                Groups.GetGroupMembers(group);
                            }
                            break;

                        default:
                            Help();
                            break;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    Console.WriteLine("[!] Command invalid: use \"Cable.exe -h\" for more details");
                    
                }
            }
            else
            {
                Console.WriteLine("[!] Cannot find module");
            }
        }
    }
}