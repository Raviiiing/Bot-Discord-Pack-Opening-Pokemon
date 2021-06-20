using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.IO;
using System.Windows;
using System.Drawing;
using Discord.WebSocket;

namespace Bot_Discord_Jeu_de_Carte.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private static List<DateTimeOffset> stackCooldownTimer = new List<DateTimeOffset>();
        private static List<SocketGuildUser> stackCooldownTarget = new List<SocketGuildUser>();
        private static Dictionary<ulong, List<DateTimeOffset>> softBan = new Dictionary<ulong, List<DateTimeOffset>>();
        private static Dictionary<ulong, int> macroTestId = new Dictionary<ulong, int>();
        private static List<ulong> bypassList = new List<ulong>();
        private int timer = 10;
        private int nbCommon = 1108;
        private int nbUnCommon = 1193;
        private int nbRare = 982;
        private int nbUltraRare = 897;
        private int nbSecrete = 104;
        private int forgeCost = 1000;
        private int newBoosterCost = 10000;
        private int secreteForgeCost = 10;

        [Command("booster", RunMode = RunMode.Sync)]
        public async Task booster()
        {
            Bdd bdd = new Bdd();
            if (Context.Message.Channel.Id is 850403180498911242 || Context.Message.Channel.Id is 853371606969221150 || Context.Message.Channel.Id is 852958994015715368 || Context.Message.Channel.Id is 852964734017077288)
            {
                if (bypassList.Contains(Context.Message.Author.Id))
                {
                    await tirage();
                }
                else if (Commands.stackCooldownTarget.Contains(Context.User as SocketGuildUser))
                {
                    if (Commands.stackCooldownTimer[Commands.stackCooldownTarget.IndexOf(Context.Message.Author as SocketGuildUser)].AddSeconds(timer) >= DateTimeOffset.Now)
                    {
                        if (macroTestId.ContainsKey(Context.Message.Author.Id) && !bypassList.Contains(Context.Message.Author.Id)) // Test deja trigger
                        {
                            await ReplyAsync(Context.Message.Author.Username + " tu est actuellement soumis à un test d'afk tape !t numero (en remplaçant le numero par : " + macroTestId[Context.Message.Author.Id] + " )");
                        }
                        else
                        {
                            int secondsLeft = (int)(Commands.stackCooldownTimer[Commands.stackCooldownTarget.IndexOf(Context.Message.Author as SocketGuildUser)].AddSeconds(timer) - DateTimeOffset.Now).TotalSeconds;
                            await ReplyAsync($"Prochain booster dans {secondsLeft} secondes <@{Context.User.Id}> ! ");
                            return;
                        }
                    }
                    else
                    {
                        Commands.stackCooldownTimer[Commands.stackCooldownTarget.IndexOf(Context.Message.Author as SocketGuildUser)] = DateTimeOffset.Now;
                        bdd.UpdateLastTime(Context.Message.Author.Id, Context.Message.Timestamp);
                        await YaTIlMacro(Context.Message.Author.Id, Context.Message.Author.Username);
                    }
                }
                else
                {
                    Commands.stackCooldownTarget.Add(Context.User as SocketGuildUser);
                    Commands.stackCooldownTimer.Add(DateTimeOffset.Now);
                    bdd.UpdateLastTime(Context.Message.Author.Id, Context.Message.Timestamp);
                    await YaTIlMacro(Context.Message.Author.Id, Context.Message.Author.Username);
                }
            }
            else
            {
                await ReplyAsync("Impossible de comunniquer ici -> Passez par le serv Officiel !");
            }
        }

        public async Task YaTIlMacro(ulong id, string username)
        {
            Bdd bdd = new Bdd();
            var rand = new Random();
            var test = new decimal(rand.Next(1, 101));
            // Test de macro 
            if (!macroTestId.ContainsKey(id) && test > 89 && !bypassList.Contains(id) && bdd.SelectMacroBooster(id) == 0) // Trigger Test
            {
                var chiffre = new decimal(rand.Next(1, 10001));
                int total = Convert.ToInt32(chiffre);
                macroTestId.Add(id, total);
                await ReplyAsync(username + " : Afk Macro test : " + total + "  - Merci de répondre avec !t *numero*");
            }
            else if (macroTestId.ContainsKey(id) && !bypassList.Contains(id)) // Test deja trigger
            {
                await ReplyAsync(username + " tu est actuellement soumis à un test d'afk tape !t numero (en remplaçant le numero par : " + macroTestId[id] + " )");
            }
            else
                await tirage();
        }

        [Command("t", RunMode = RunMode.Sync)]

        public async Task testMacro(string saisie)
        {
            int i;
            Bdd bdd = new Bdd();
            if (saisie == null)
            {
                await ReplyAsync("Aucun parmetre retourné");
                return;
            }
            if (Int32.TryParse(saisie, out i))
            {
                if (macroTestId.ContainsKey(Context.Message.Author.Id))
                {
                    if (Int32.Parse(saisie) == macroTestId[Context.Message.Author.Id])
                    {
                        await ReplyAsync(Context.Message.Author.Username + " : Test Anti macro réussi bon jeu !");
                        bdd.UpdateMacroBooster(Context.Message.Author.Id, 4);
                        macroTestId.Remove(Context.Message.Author.Id);
                        await tirage();
                    }
                    else
                    {
                        await ReplyAsync(Context.Message.Author.Username + " : Ce chiffre ne correspond pas");
                        return;
                    }
                }
                else
                {
                    await ReplyAsync(Context.Message.Author.Username + " : Vous n'avez pas de test anti macro activé ");
                    return;
                }
            }
            else if (macroTestId.ContainsKey(Context.Message.Author.Id))
            {
                await ReplyAsync(Context.Message.Author.Username + " : le parametre retourné ne correspond pas à un chiffre");
                return;
            }
            else
            {
                await ReplyAsync(Context.Message.Author.Username + " : Vous n'avez pas de test anti macro activé ");
                return;
            }
        }

        public async Task tirage()
        {
            Bdd bdd = new Bdd();
            ulong user_id = Context.Message.Author.Id;
            string username = Context.Message.Author.Username;
            bool secrete = false;
            string[,] deck = new string[2, 10];
            var rand = new Random();
            int moneyToAdd = 0;
            if (!(bdd.Select(user_id)))
            {
                bdd.AddUser(user_id, username);
            }
            try
            {
                for (int i = 0; i < 6; i++)
                {
                    var randNumber = new decimal(rand.Next(1, nbCommon + 1));
                    deck[0, i] = @"Common\" + randNumber + ".jpg";
                    deck[1, i] = randNumber.ToString();
                    if (bdd.IsCardInTable(randNumber.ToString(), user_id, "user_common_list"))
                    {
                        string nb_card = bdd.SelectCard(randNumber.ToString(), user_id, "user_common_list");
                        bdd.UpdateList(randNumber.ToString(), user_id, "user_common_list", nb_card);
                        moneyToAdd += 1;
                    }
                    else
                        bdd.AddCardToUser(randNumber.ToString(), user_id, "user_common_list");
                }

                for (int i = 6; i < 9; i++)
                {
                    var randNumber = new decimal(rand.Next(1, nbUnCommon + 1));
                    deck[0, i] = @"Uncommon\" + randNumber + ".jpg";
                    deck[1, i] = randNumber.ToString();
                    if (bdd.IsCardInTable(randNumber.ToString(), user_id, "user_uncommon_list"))
                    {
                        string nb_card = bdd.SelectCard(randNumber.ToString(), user_id, "user_uncommon_list");
                        bdd.UpdateList(randNumber.ToString(), user_id, "user_uncommon_list", nb_card);
                        moneyToAdd += 5;
                    }
                    else
                        bdd.AddCardToUser(randNumber.ToString(), user_id, "user_uncommon_list");
                }

                while (deck[0, 9] == null)
                {
                    var randNumber = new decimal(rand.Next(0, 201));
                    if (randNumber < 140)
                    {
                        randNumber = new decimal(rand.Next(1, nbRare + 1));
                        deck[0, 9] = @"Rare\" + randNumber + ".jpg";
                        if (bdd.IsCardInTable(randNumber.ToString(), user_id, "user_rare_list"))
                        {
                            string nb_card = bdd.SelectCard(randNumber.ToString(), user_id, "user_rare_list");
                            bdd.UpdateList(randNumber.ToString(), user_id, "user_rare_list", nb_card);
                            moneyToAdd += 25;
                        }
                        else
                            bdd.AddCardToUser(randNumber.ToString(), user_id, "user_rare_list");
                    }
                    else if (randNumber >= 140 && randNumber < 196)
                    {
                        randNumber = new decimal(rand.Next(1, nbUltraRare + 1));
                        deck[0, 9] = @"Ultra_rare\" + randNumber + ".jpg";
                        if (bdd.IsCardInTable(randNumber.ToString(), user_id, "user_rare_ultra_list"))
                        {
                            string nb_card = bdd.SelectCard(randNumber.ToString(), user_id, "user_rare_ultra_list");
                            bdd.UpdateList(randNumber.ToString(), user_id, "user_rare_ultra_list", nb_card);
                            moneyToAdd += 100;
                        }
                        else
                            bdd.AddCardToUser(randNumber.ToString(), user_id, "user_rare_ultra_list");
                    }
                    else if (randNumber >= 196 && randNumber < 201)
                    {
                        randNumber = new decimal(rand.Next(1, nbSecrete + 1));
                        deck[0, 9] = @"Secrete\" + randNumber + ".jpg";
                        secrete = true;
                        if (bdd.IsCardInTable(randNumber.ToString(), user_id, "user_secret_list"))
                        {
                            string nb_card = bdd.SelectCard(randNumber.ToString(), user_id, "user_secret_list");
                            bdd.UpdateList(randNumber.ToString(), user_id, "user_secret_list", nb_card);
                            moneyToAdd += 1000;
                        }
                        else
                            bdd.AddCardToUser(randNumber.ToString(), user_id, "user_secret_list");
                    }
                    deck[1, 9] = randNumber.ToString();
                    if (moneyToAdd > 0)
                    {
                        int pastAmont = bdd.SelectMoney(user_id);
                        bdd.AddMoney(user_id, moneyToAdd, pastAmont);
                    }
                }
                if (bdd.SelectMacroBooster(Context.Message.Author.Id) > 0)
                {
                    bdd.UpdateMacroBooster(user_id, bdd.SelectMacroBooster(Context.Message.Author.Id) - 1);
                }
                string[] fichiers = new string[5];
                fichiers[0] = deck[0, 0];
                fichiers[1] = deck[0, 1];
                fichiers[2] = deck[0, 2];
                fichiers[3] = deck[0, 3];
                fichiers[4] = deck[0, 4];
                Image.CombineBitmapL(fichiers).Save(@"top.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                fichiers[0] = deck[0, 5];
                fichiers[1] = deck[0, 6];
                fichiers[2] = deck[0, 7];
                fichiers[3] = deck[0, 8];
                fichiers[4] = deck[0, 9];
                Image.CombineBitmapL(fichiers).Save(@"bottom.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                string[] fichiers2 = new string[2];
                fichiers2[0] = @"top.jpg";
                fichiers2[1] = @"bottom.jpg";
                Image.CombineBitmapH(fichiers2).Save(@"board.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);


                var filename = "board.jpg";
                var embedBuilder = new EmbedBuilder();
                if (secrete)
                {
                    embedBuilder.WithTitle("SECRETE !");
                }
                embedBuilder.WithImageUrl($"attachment://{filename}");
                embedBuilder.AddField("Booster de :", Context.User);
                if (moneyToAdd > 0)
                    embedBuilder.AddField("Nouveau Solde: ", bdd.SelectMoney(user_id));
                embedBuilder.WithFooter("Envoyé à " + DateTime.Now.ToString());
                embedBuilder.WithColor(new Discord.Color(54, 57, 62));

                Embed embed = embedBuilder.Build();
                await Context.Channel.SendFileAsync(filename, embed: embed);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Aucune erreur commande executée !");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            GC.Collect();
        }

        [Command("inv", RunMode = RunMode.Async)]

        public async Task Inventory()
        {
            Bdd bdd = new Bdd();
            ulong user_id = Context.Message.Author.Id;
            string username = Context.Message.Author.Username;
            if (!(bdd.Select(user_id)))
            {
                await ReplyAsync("Pack au moins une carte pélo !");
            }
            else
            {
                await ReplyAsync("Bonjour " + username + ", je t'invite à cliquer sur ce lien et de choisir ton compte ! http://raviing.freeboxos.fr/lucasfdp/blank.php");
            }
        }

        public void softBanId(ulong id)
        {
            List<DateTimeOffset> dateDeBan = new List<DateTimeOffset>();
            if (!softBan.ContainsKey(id))
            {
                softBan.Add(id, new List<DateTimeOffset>());
            }
            else if (softBan[id].Count == 0)
            {
                softBan[id].Add(Context.Message.Timestamp);
            }
            else if (softBan[id].Count > 0)
            {
                dateDeBan = softBan[id];
                if ((DateTimeOffset.Now - dateDeBan.First()).TotalHours > 1)
                {
                    softBan.Remove(id);
                }
            }
        }

        [Command("bypass", RunMode = RunMode.Sync)]
        public async Task Bypass(IGuildUser user = null)
        {
            if (Context.Message.Author.Id is 273808029366091776)
            {
                if (user == null)
                {
                    await ReplyAsync("Please specify a user !");
                }
                else if (user is IGuildUser)
                {
                    if (bypassList.Contains(user.Id))
                    {
                        bypassList.Remove(user.Id);
                        await ReplyAsync(user.Username + " -> Droit priviléges retiré par : " + Context.User.Username);
                    }
                    else
                    {
                        bypassList.Add(user.Id);
                        await ReplyAsync(user.Username + " -> Droit de Bypass de Cooldown accordé par : " + Context.User.Username);
                    }
                }
                else
                    await ReplyAsync(Context.Message.Author.Username + " -> Commande !info paramétre inconnu");
            }
            else
                await ReplyAsync("Ptdr t ki ?");
        }

        [Command("aide")]

        public async Task Info()
        {
            var embed = new EmbedBuilder();
            var footer = new EmbedFooterBuilder();
            footer.WithText("Envoyé le " + DateTime.Now.ToString());
            embed.WithTitle("Commande help !")
                 .AddField(" !booster ", "-> te permet d'ouvrir un booster toutes les " + timer + " secondes ")
                 .AddField(" !t chiffre ", " -> te permet de completer la requête de test anti afk (le chiffre sera fourni)")
                 .AddField(" !inv ", "-> te permet de voir l'avancée de ta collection ! Le lien t'envoi vers l'aperçu visuel de ta collection")
                 .AddField(" !forge ", "-> Cette commande te coûte 1000 pièces et permet de forger une carte que tu n'as pas !")
                 .AddField(" !newbooster", "-> Cette commande te coûte 10000 pièces. Meme principe que la forge mais avec 10 cartes ! ")
                 .AddField(" !forgesecrete ", "-> Cette commande te coûte 100 000 pièces et te permet de crafter une secréte que tu n'as pas !")
                 .AddField(" !money ", "-> Permet de voir combien tu as pièces !")
                 .WithFooter(footer);
            Embed embedBuilder = embed.Build();
            await Context.Channel.SendMessageAsync(embed: embedBuilder);
        }

        [Command("displayall")]

        public async Task DisplayAll()
        {
            if (Context.Message.Author.Id is 273808029366091776)
            {
                Bdd bdd = new Bdd();
                List<string> result = bdd.SelectAll();
                var embed = new EmbedBuilder();
                var footer = new EmbedFooterBuilder();
                Console.WriteLine("Nb éléments : {0}", result.Count);

                EmbedFieldBuilder field = new EmbedFieldBuilder();
                field.WithName("Liste de utilisateurs inscrits :");

                for (int x = 0; x < result.Count; x++)
                {
                    field.WithValue(result[x]);
                }
                embed.AddField(field);
                Embed embedbuilder = embed.Build();
                await Context.Channel.SendMessageAsync(embed: embedbuilder);
            }
            else
                await ReplyAsync("Tu n'est pas administrateur ");
        }

        [Command("boobster")]

        public async Task Boobster()
        {
            if (Context.Message.Author.Id == 309728387176595456 || Context.Message.Author.Id == 273808029366091776)
            {
                await Context.Channel.SendFileAsync("img/boobster.png");
            }
            else
                await ReplyAsync("ERROR 69 : Vous n'avez pas le saint pouvoir du boobster");
        }

        [Command("newbooster")]

        public async Task NewBooster()
        {
            Bdd bdd = new Bdd();
            int UserMoney = bdd.SelectMoney(Context.Message.Author.Id);

            ulong user_id = Context.Message.Author.Id;
            string username = Context.Message.Author.Username;

            string[,] deck = new string[2, 10];
            var rand = new Random();

            if (UserMoney >= newBoosterCost)
            {
                int commonUser = bdd.SelectCountCard(Context.Message.Author.Id, "user_common_list");
                int unCommonUser = bdd.SelectCountCard(Context.Message.Author.Id, "user_uncommon_list");
                int RareUser = bdd.SelectCountCard(Context.Message.Author.Id, "user_rare_list");
                int UltraRareUser = bdd.SelectCountCard(Context.Message.Author.Id, "user_rare_ultra_list");
                int total = commonUser + unCommonUser + RareUser + UltraRareUser;
                if (total == nbCommon + nbUnCommon + nbRare + nbUltraRare)
                {
                    await ReplyAsync("Vous avez terminé la collection ! Ce booster n'est plus disponible");
                }
                else if (((nbCommon + nbUnCommon + nbRare + nbUltraRare) - total) <= 9)
                {
                    await ReplyAsync("Il vous reste moins de 10 cartes - Impossible d'ouvrir un booster");
                }
                else if (((nbCommon + nbUnCommon + nbRare + nbUltraRare) - total) > 9)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        bool commonFULL = bdd.SelectCountCard(Context.Message.Author.Id, "user_common_list") == nbCommon;
                        bool uncommonFULL = bdd.SelectCountCard(Context.Message.Author.Id, "user_uncommon_list") == nbUnCommon;
                        bool rareFULL = bdd.SelectCountCard(Context.Message.Author.Id, "user_rare_list") == nbRare;
                        bool ultra_rareFULL = bdd.SelectCountCard(Context.Message.Author.Id, "user_rare_ultra_list") == nbUltraRare;
                        var randNumber = new decimal(rand.Next(0, 1001));
                        Console.WriteLine("Rarete = {0}", randNumber);
                        if (randNumber <= 250 && commonFULL == false)
                        {
                            do
                            {
                                randNumber = new decimal(rand.Next(1, nbCommon + 1));
                            } while (bdd.IsCardInTable(randNumber.ToString(), user_id, "user_common_list"));
                            deck[0, i] = @"Common\" + randNumber + ".jpg";
                            deck[1, i] = randNumber.ToString();
                            bdd.AddCardToUser(randNumber.ToString(), user_id, "user_common_list");
                        }
                        else if (randNumber > 250 && randNumber <= 500 && uncommonFULL == false)
                        {
                            do
                            {
                                randNumber = new decimal(rand.Next(1, nbUnCommon + 1));
                            } while (bdd.IsCardInTable(randNumber.ToString(), user_id, "user_uncommon_list"));
                            deck[0, i] = @"Uncommon\" + randNumber + ".jpg";
                            deck[1, i] = randNumber.ToString();
                            bdd.AddCardToUser(randNumber.ToString(), user_id, "user_uncommon_list");
                        }
                        else if (randNumber > 500 && randNumber <= 750 && rareFULL == false)
                        {
                            do
                            {
                                randNumber = new decimal(rand.Next(1, nbRare + 1));
                            } while (bdd.IsCardInTable(randNumber.ToString(), user_id, "user_rare_list"));
                            deck[0, i] = @"Rare\" + randNumber + ".jpg";
                            deck[1, i] = randNumber.ToString();
                            bdd.AddCardToUser(randNumber.ToString(), user_id, "user_rare_list");
                        }
                        else if (randNumber > 750 && ultra_rareFULL == false)
                        {
                            do
                            {
                                randNumber = new decimal(rand.Next(1, nbUltraRare + 1));
                            } while (bdd.IsCardInTable(randNumber.ToString(), user_id, "user_rare_ultra_list"));
                            deck[0, i] = @"Ultra_rare\" + randNumber + ".jpg";
                            deck[1, i] = randNumber.ToString();
                            bdd.AddCardToUser(randNumber.ToString(), user_id, "user_rare_ultra_list");
                        }
                        else if (randNumber > 750 && ultra_rareFULL == true)
                        {
                            i--;
                        }
                        else if (randNumber > 500 && randNumber <= 750 && rareFULL == true)
                        {
                            i--;
                        }
                        else if (randNumber > 250 && randNumber <= 500 && uncommonFULL == true)
                        {
                            i--;
                        }
                        else if (randNumber <= 250 && commonFULL == true)
                        {
                            i--;
                        }
                    }
                    bdd.RemoveMoney(user_id, forgeCost, bdd.SelectMoney(user_id));
                    string[] fichiers = new string[5];
                    fichiers[0] = deck[0, 0];
                    fichiers[1] = deck[0, 1];
                    fichiers[2] = deck[0, 2];
                    fichiers[3] = deck[0, 3];
                    fichiers[4] = deck[0, 4];
                    Image.CombineBitmapL(fichiers).Save(@"top.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                    fichiers[0] = deck[0, 5];
                    fichiers[1] = deck[0, 6];
                    fichiers[2] = deck[0, 7];
                    fichiers[3] = deck[0, 8];
                    fichiers[4] = deck[0, 9];
                    Image.CombineBitmapL(fichiers).Save(@"bottom.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                    string[] fichiers2 = new string[2];
                    fichiers2[0] = @"top.jpg";
                    fichiers2[1] = @"bottom.jpg";
                    Image.CombineBitmapH(fichiers2).Save(@"board.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);


                    var filename = "board.jpg";
                    var embedBuilder = new EmbedBuilder();
                    embedBuilder.WithImageUrl($"attachment://{filename}");
                    embedBuilder.AddField("Booster de :", Context.User);
                    embedBuilder.AddField("Monnaie : ", bdd.SelectMoney(user_id));
                    embedBuilder.WithFooter("Envoyé à " + DateTime.Now.ToString());
                    embedBuilder.WithColor(new Discord.Color(54, 57, 62));

                    Embed embed = embedBuilder.Build();
                    await Context.Channel.SendFileAsync(filename, embed: embed);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Aucune erreur commande executée !");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            else
            {
                await ReplyAsync("Pas assez d'argent ! Vous avez : " + UserMoney + " piéces sur les " + newBoosterCost + " demandé");
            }
        }

        [Command("forge")]

        public async Task Forge()
        {
            var rand = new Random();
            Bdd bdd = new Bdd();
            ulong user_id = Context.Message.Author.Id;
            int UserMoney = bdd.SelectMoney(user_id);

            bool commonFULL = bdd.SelectCountCard(Context.Message.Author.Id, "user_common_list") == nbCommon;
            bool uncommonFULL = bdd.SelectCountCard(Context.Message.Author.Id, "user_uncommon_list") == nbUnCommon;
            bool rareFULL = bdd.SelectCountCard(Context.Message.Author.Id, "user_rare_list") == nbRare;
            bool ultra_rareFULL = bdd.SelectCountCard(Context.Message.Author.Id, "user_rare_ultra_list") == nbUltraRare;

            int commonUser = bdd.SelectCountCard(Context.Message.Author.Id, "user_common_list");
            int unCommonUser = bdd.SelectCountCard(Context.Message.Author.Id, "user_uncommon_list");
            int RareUser = bdd.SelectCountCard(Context.Message.Author.Id, "user_rare_list");
            int UltraRareUser = bdd.SelectCountCard(Context.Message.Author.Id, "user_rare_ultra_list");
            int total = commonUser + unCommonUser + RareUser + UltraRareUser;
            string cardURL = null;

            if (UserMoney < forgeCost)
            {
                await ReplyAsync("Pas assez d'argent ! Vous avez : " + UserMoney + " piéces sur les " + forgeCost + " demandé");
            }
            else if (UserMoney >= 250 && total < (nbCommon + nbUnCommon + nbRare + nbUltraRare))
            {
                do
                {
                    var randNumber = new decimal(rand.Next(0, 1001));
                    Console.WriteLine("Rarete = {0}", randNumber);
                    if (randNumber <= 250 && commonFULL == false)
                    {
                        do
                        {
                            randNumber = new decimal(rand.Next(1, nbCommon + 1));
                        } while (bdd.IsCardInTable(randNumber.ToString(), user_id, "user_common_list"));
                        cardURL = @"Common\" + randNumber + ".jpg";
                        bdd.AddCardToUser(randNumber.ToString(), user_id, "user_common_list");
                    }
                    else if (randNumber > 250 && randNumber <= 500 && uncommonFULL == false)
                    {
                        do
                        {
                            randNumber = new decimal(rand.Next(1, nbUnCommon + 1));
                        } while (bdd.IsCardInTable(randNumber.ToString(), user_id, "user_uncommon_list"));
                        cardURL = @"Uncommon\" + randNumber + ".jpg";
                        bdd.AddCardToUser(randNumber.ToString(), user_id, "user_uncommon_list");
                    }
                    else if (randNumber > 500 && randNumber <= 750 && rareFULL == false)
                    {
                        do
                        {
                            randNumber = new decimal(rand.Next(1, nbRare + 1));
                        } while (bdd.IsCardInTable(randNumber.ToString(), user_id, "user_rare_list"));
                        cardURL = @"Rare\" + randNumber + ".jpg";
                        bdd.AddCardToUser(randNumber.ToString(), user_id, "user_rare_list");
                    }
                    else if (randNumber > 750 && ultra_rareFULL == false)
                    {
                        do
                        {
                            randNumber = new decimal(rand.Next(1, nbUltraRare + 1));
                        } while (bdd.IsCardInTable(randNumber.ToString(), user_id, "user_rare_ultra_list"));
                        cardURL = @"Ultra_Rare\" + randNumber + ".jpg";
                        bdd.AddCardToUser(randNumber.ToString(), user_id, "user_rare_ultra_list");
                    }
                } while (cardURL is null);
                bdd.RemoveMoney(user_id, forgeCost, bdd.SelectMoney(user_id));

                var embedBuilder = new EmbedBuilder();
                embedBuilder.AddField("Carte forgée de :", Context.User);
                embedBuilder.WithImageUrl($"attachment://{cardURL}");
                embedBuilder.AddField("Monnaie : ", bdd.SelectMoney(user_id));
                embedBuilder.WithFooter("Envoyé à " + DateTime.Now.ToString());
                embedBuilder.WithColor(new Discord.Color(54, 57, 62));

                Embed embed = embedBuilder.Build();
                await Context.Channel.SendFileAsync(cardURL, embed: embed);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Aucune erreur commande executée !");
                Console.ForegroundColor = ConsoleColor.White;
                GC.Collect();
            }
            else if (UserMoney >= 1000 && total == (nbCommon + nbUnCommon + nbRare + nbUltraRare))
            {
                await ReplyAsync("Vous avez terminé la collection !");
            }
        }

        [Command("forgesecrete")]

        public async Task ForgeSecrete()
        {
            ulong user_id = Context.Message.Author.Id;
            string username = Context.Message.Author.Username;
            Bdd bdd = new Bdd();
            if (bdd.SelectMoney(user_id) < secreteForgeCost)
            {
                await ReplyAsync("Pas assez d'argent pour forger une secrete ! " + bdd.SelectMoney(user_id) + "/" + secreteForgeCost);
            }
            else
            {
                bool secreteFull = bdd.SelectCountCard(user_id, "user_secret_list") == nbSecrete;
                if (!secreteFull)
                {
                    var rand = new Random();
                    var randNumber = new decimal(rand.Next(1, nbSecrete + 1));
                    string cardURL = "";
                    do
                    {
                        randNumber = new decimal(rand.Next(1, nbSecrete + 1));
                        cardURL = randNumber.ToString();
                    } while (bdd.IsCardInTable(cardURL, user_id, "user_secret_list"));
                    cardURL = @"Secrete\" + randNumber + ".jpg";
                    bdd.AddCardToUser(randNumber.ToString(), user_id, "user_secret_list");
                    bdd.RemoveMoney(user_id, secreteForgeCost, bdd.SelectMoney(user_id));

                    var embedBuilder = new EmbedBuilder();
                    embedBuilder.AddField("Carte SECRETE forgée de :", Context.User);
                    embedBuilder.WithImageUrl($"attachment://{cardURL}");
                    embedBuilder.AddField("Monnaie : ", bdd.SelectMoney(user_id));
                    embedBuilder.WithFooter("Envoyé à " + DateTime.Now.ToString());
                    embedBuilder.WithColor(new Discord.Color(54, 57, 62));

                    Embed embed = embedBuilder.Build();
                    await Context.Channel.SendFileAsync(cardURL, embed: embed);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Aucune erreur commande executée !");
                    Console.ForegroundColor = ConsoleColor.White;
                    GC.Collect();
                }
                else if (secreteFull)
                {
                    await ReplyAsync("GG vous avez déjà toutes les cartes secretes ! Impossible de faire cette commande !");
                }
            }
        }

        [Command("setmoney")]

        public async Task SetMoney(IGuildUser user)
        {
            if (Context.Message.Author.Id is 273808029366091776)
            {
                if (user == null)
                {
                    await ReplyAsync("Please specify a user !");
                }
                else if (user is IGuildUser)
                {
                    ulong user_id = user.Id;
                    string username = user.Username;
                    Bdd bdd = new Bdd();
                    int duplicateCommon = bdd.SelectDuplicateCount(user_id, "user_common_list") - bdd.SelectCountCard(user_id, "user_common_list");
                    int duplicateUnCommon = bdd.SelectDuplicateCount(user_id, "user_uncommon_list") - bdd.SelectCountCard(user_id, "user_uncommon_list");
                    int duplicateRare = bdd.SelectDuplicateCount(user_id, "user_rare_list") - bdd.SelectCountCard(user_id, "user_rare_list");
                    int duplicateUltraRare = bdd.SelectDuplicateCount(user_id, "user_rare_ultra_list") - bdd.SelectCountCard(user_id, "user_rare_ultra_list");
                    int duplicateSecrete = bdd.SelectDuplicateCount(user_id, "user_secret_list") - bdd.SelectCountCard(user_id, "user_secret_list");

                    int total = duplicateCommon * 1 + duplicateUnCommon * 5 + duplicateRare * 25 + duplicateUltraRare * 100 + duplicateSecrete * 1000;

                    bdd.AddMoney(user_id, total, 0);

                    await ReplyAsync("Solde de " + username + " updated : " + total);
                }
                else
                    await ReplyAsync(Context.Message.Author.Username + " -> Commande !info paramétre inconnu");
            }
            else
                await ReplyAsync("Ptdr t ki ?");
        }

        [Command("money")]

        public async Task ShowMoney()
        {
            ulong user_id = Context.Message.Author.Id;
            string username = Context.Message.Author.Username;
            Bdd bdd = new Bdd();
            await ReplyAsync(username + " votre solde est de " + bdd.SelectMoney(user_id));
        }
    }
}