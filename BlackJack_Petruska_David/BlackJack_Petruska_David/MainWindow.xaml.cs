using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;

namespace BlackJack_Petruska_David
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Ints for total points for all three hands, and the current and previous cards
        private int userCardTotal = 0, splitCardTotal = 0, dealerCardTotal = 0, currentCard, cardPoints = 0, previousCard, dealerSecondCard = 0;

        // Ints used to handle betting
        private int playerTotal = 0, currentBet = 0, place1 = 0, place10 = 0, place100 = 0, place500 = 0, betTotal = 0, doubleDownBet = 0, differenceBet = 0;

        // Booleans used for win/lose, and game state
        private Boolean win = false, push = false, split = false, splitWin = false, splitPush = false, blackjackHand = false, splitHandBlackjack = false, surrender = false, goodSurrender = false;
        private Boolean newUserLogged = false;

        // used to log in and save balance changes
        private String userLoggedIn, line, filePath, newUserLine = "";

        // file reader
        FileStream file; StreamReader StreamReader; StreamWriter StreamWriter;
        private List<String> lines, matchList;
        // default deck
        private List<int> deck = new List<int>{ 1, 2, 3, 4, 5, 6, 7, 8, 9,
                                        10, 11, 12, 13, 14, 15, 16,
                                        17, 18, 19, 20, 21, 22, 23,
                                        24, 25, 26, 27, 28, 29, 30,
                                        31, 32, 34, 35, 36, 37, 38,
                                        39, 40, 41, 42, 43, 44, 45,
                                        46, 47, 48, 49, 50, 51, 52 };
        // used as the shuffled deck to play with
        private List<int> shuffledDeck, userHand = new List<int> { 0 }, dealerHand = new List<int> { 0 };

        // used to get the cards worth
        private int[] aces = new int[] { 1, 14, 27, 40 };
        private int[] twos = new int[] { 2, 15, 28, 41 };
        private int[] threes = new int[] { 3, 16, 29, 42 };
        private int[] fours = new int[] { 4, 17, 30, 43 };
        private int[] fives = new int[] { 5, 18, 31, 44 };
        private int[] sixes = new int[] { 6, 19, 32, 45 };
        private int[] sevens = new int[] { 7, 20, 33, 46 };
        private int[] eights = new int[] { 8, 21, 34, 47 };
        private int[] nines = new int[] { 9, 22, 35, 48 };
        private int[] tens = new int[] { 10, 23, 36, 49 };
        private int[] jacks = new int[] { 11, 24, 37, 50 };
        private int[] queens = new int[] { 12, 25, 38, 51 };
        private int[] kings = new int[] { 13, 26, 39, 52 };

        // Ints used to count the cards in the hands
        private int userCardCount = 2, splitCardCount = 2, dealerCardCount = 2;

        public List<int> ShuffledDeck { get => ShuffledDeck1; set => ShuffledDeck1 = value; }
        public List<int> ShuffledDeck1 { get => shuffledDeck; set => shuffledDeck = value; }

        public MainWindow()
        {
            InitializeComponent();
            NewDeck();
            PreBetFelt();
            SplitHitButton.Visibility = System.Windows.Visibility.Hidden;
        }

        // Handles the hitting to the main hand, 
        // if the user busts, throws control
        // to the dealer.
        private void HitButton_Click(object sender, RoutedEventArgs e)
        {
            DoubleDownButton.Visibility = Visibility.Hidden;
            HitButton = (Button)sender;
            Hit();
            userHand.Add(currentCard);
            userCardTotal += CardWorth(currentCard);
            BitmapImage image = AssignFaceValue();


            if (userCardCount == 2)
            {
                userCard3.Source = image;
            }
            if (userCardCount == 3)
            {
                userCard4.Source = image;
            }
            if (userCardCount == 4)
            {
                userCard5.Source = image;
            }
            if (userCardCount == 5)
                userCard6.Source = image;

            userPoints.Content = userCardTotal;
            userCardCount++;
            if (userCardTotal > 21)
            {
                HitButton.IsEnabled = false;
                Stand();
            }
            if (userCardTotal == 21)
                blackjackHand = true;
        }

        // Calls the Stand() function
        private void StandButton_Click(object sender, RoutedEventArgs e)
        {
            Stand();
        }

        // Used to execute the login functiondd
        private void Login_Listener(object sender, RoutedEventArgs e)
        {
            UserLogin();
        }

        // used to save the balance to the user in the file
        // that is used along with this program
        private void SaveUserData_Click(object sender, RoutedEventArgs e)
        {
            SaveBalance();
        }

        private void SurrenederButton_Click(object sender, RoutedEventArgs e)
        {
            Surrender();
            surrender = true;
        }

        // Handles the hitting on the split hand,
        // and disables the button when the user
        // busts.
        private void SplitHitButton_Click(object sender, RoutedEventArgs e)
        {
            HitButton = (Button)sender;
            Hit();
            userHand.Add(currentCard);
            splitCardTotal += CardWorth(currentCard);
            BitmapImage image = AssignFaceValue();


            if (splitCardCount == 2)
            {
                splitCard3.Source = image;
            }
            if (splitCardCount == 3)
            {
                splitCard4.Source = image;
            }
            if (splitCardCount == 4)
            {
                splitCard5.Source = image;
            }
            if (splitCardCount == 5)
                splitCard6.Source = image;

            splitHandPoints.Content = splitCardTotal;
            splitCardCount++;
            if (splitCardTotal > 21)
                HitButton.IsEnabled = false;

            if (splitCardTotal == 21 && (splitCardCount == 2))
                splitHandBlackjack = true;
        }

        // Handles the main hands stand,
        // deals the dealer their cards,
        // and handles the split hand
        private void Stand()
        {
            HitButton.IsEnabled = false;
            DealerHit();
            NextHand.IsEnabled = true;

            if (split)
            {
                SplitHitButton.IsEnabled = false;
                splitBetStatus.Content = SplitBetStatus();

                if (splitWin)
                    playerTotal = playerTotal + (currentBet * 2);
                if (splitPush)
                    playerTotal = playerTotal + currentBet;
                if (splitHandBlackjack && (userCardCount == 2))
                    playerTotal = playerTotal + (currentBet * 3);

                userBalance.Content = playerTotal;
            }

            betStatusLabel.Content = BetStatus();
        }

        // Assigns the images to the card number.
        private BitmapImage AssignFaceValue()
        {
            BitmapImage faceValue = new BitmapImage();
            faceValue.BeginInit();
            switch (currentCard)
            {
                case 1:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/1.gif"));
                    break;
                case 2:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/2.gif"));
                    break;
                case 3:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/3.gif"));
                    break;
                case 4:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/4.gif"));
                    break;
                case 5:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/5.gif"));
                    break;
                case 6:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/6.gif"));
                    break;
                case 7:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/7.gif"));
                    break;
                case 8:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/8.gif"));
                    break;
                case 9:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/9.gif"));
                    break;
                case 10:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/10.gif"));
                    break;
                case 11:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/11.gif"));
                    break;
                case 12:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/12.gif"));
                    break;
                case 13:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/13.gif"));
                    break;

                case 14:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/14.gif"));
                    break;
                case 15:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/15.gif"));
                    break;
                case 16:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/16.gif"));
                    break;
                case 17:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/17.gif"));
                    break;
                case 18:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/18.gif"));
                    break;
                case 19:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/19.gif"));
                    break;
                case 20:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/20.gif"));
                    break;
                case 21:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/21.gif"));
                    break;
                case 22:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/22.gif"));
                    break;
                case 23:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/23.gif"));
                    break;
                case 24:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/24.gif"));
                    break;
                case 25:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/25.gif"));
                    break;

                case 26:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/26.gif"));
                    break;
                case 27:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/27.gif"));
                    break;
                case 28:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/28.gif"));
                    break;
                case 29:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/29.gif"));
                    break;
                case 30:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/30.gif"));
                    break;
                case 31:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/31.gif"));
                    break;
                case 32:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/32.gif"));
                    break;
                case 33:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/33.gif"));
                    break;
                case 34:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/34.gif"));
                    break;
                case 35:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/35.gif"));
                    break;
                case 36:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/36.gif"));
                    break;
                case 37:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/37.gif"));
                    break;
                case 38:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/38.gif"));
                    break;
                case 39:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/39.gif"));
                    break;

                case 40:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/40.gif"));
                    break;
                case 41:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/41.gif"));
                    break;
                case 42:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/42.gif"));
                    break;
                case 43:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/43.gif"));
                    break;
                case 44:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/44.gif"));
                    break;
                case 45:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/45.gif"));
                    break;
                case 46:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/46.gif"));
                    break;
                case 47:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/47.gif"));
                    break;
                case 48:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/48.gif"));
                    break;
                case 49:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/49.gif"));
                    break;
                case 50:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/50.gif"));
                    break;
                case 51:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/51.gif"));
                    break;
                case 52:
                    faceValue.UriSource = (new Uri("pack://application:,,,/Images/52.gif"));
                    break;

                default:
                    faceValue = null;
                    break;
            }

            faceValue.EndInit();


            return faceValue;
        }

        // Get the next card out of the shuffled deck.
        // Gets a new deck once the size is less than 15.
        private int Hit()
        {
            if (ShuffledDeck.Count <= 15)
            {
                NewDeck();
                DeckStatus.Content = "Shuffled New Deck";
            }
            currentCard = ShuffledDeck[0];
            ShuffledDeck.Remove(currentCard);

            return currentCard;
        }

        // Calls the NextHand() function to reset the felt
        // and move on to the next bet placement after
        // dealing with pot.
        private void NextHand_Listener(object sender, RoutedEventArgs e)
        {
            if (win && !blackjackHand)
                playerTotal = playerTotal + (currentBet * 2);
            if (push)
                playerTotal = playerTotal + currentBet;
            if (blackjackHand && win)
                playerTotal = playerTotal + (currentBet * 3);
            if (goodSurrender)
                playerTotal = playerTotal + (currentBet / 2);

            NewHand();
            userBalance.Content = playerTotal;
            currentBet = 0;
            userBet.Content = currentBet;
            ResetFelt();
        }

        // Calls the SplitHand() function and set the 
        // containers to show a split.
        private void SplitEngage(object sender, RoutedEventArgs e)
        {
            userTagSplit.Content = "Split Hand";
            SplitHitButton.Visibility = System.Windows.Visibility.Visible;
            SplitHitButton.IsEnabled = true;
            SplitHand();
            split = true;
        }

        // Listener for the placing of 1 to the pot.
        private void Place1_Listener(object sender, RoutedEventArgs e)
        {
            Button Place1 = (Button)sender;

            place1++;

            playerTotal--;
            userBalance.Content = playerTotal;
        }

        // Listener for the placing of 10 to the pot.
        private void Place10_Listener(object sender, RoutedEventArgs e)
        {
            Button Place10 = (Button)sender;

            place10 += place10 + 10;
            playerTotal = playerTotal - 10;
            userBalance.Content = playerTotal;
        }

        // Calls the DoubleDown() function when clicked.
        private void DoubleDown_Listener(object sender, RoutedEventArgs e)
        {
            DoubleDown();
        }

        // Listener for the placing fo 100 to the pot.
        private void Place100_Listener(object sender, RoutedEventArgs e)
        {
            Button Place100 = (Button)sender;

            place100 = place100 + 100;
            playerTotal = playerTotal - 100;
            userBalance.Content = playerTotal;
        }

        // listener for the placing of 500 to the pot.
        private void Place500_Listener(object sender, RoutedEventArgs e)
        {
            Button Place500 = (Button)sender;

            place500 = place500 + 500;
            playerTotal = playerTotal - 500;
            userBalance.Content = playerTotal;
        }

        // Defaults the player bank to 2000
        private void GuestPlaying(object sender, RoutedEventArgs e)
        {
            playerTotal = 2000;
            userBalance.Content = playerTotal;
        }

        // Collects the bets from the above functions
        // and displays the total in to the label.
        private void PlaceBet_Listener(object sender, RoutedEventArgs e)
        {
            Place1.IsEnabled = false;
            Place10.IsEnabled = false;
            Place100.IsEnabled = false;
            Place500.IsEnabled = false;
            betTotal = place1 + place10 + place100 + place500;
            currentBet = betTotal;
            userBet.Content = currentBet;
            PostBetFelt();
            NewHand();
        }

        // Function used to place cards
        // to dealer hand. Based on a loop
        // and a conditional that the user
        // has not gone bust.
        private void DealerHit()
        {
            currentCard = dealerSecondCard;
            dealerCardTotal = dealerCardTotal + CardWorth(dealerSecondCard);
            dealerPoints.Content = dealerCardTotal;
            BitmapImage image = AssignFaceValue();
            dealerCard2.Source = image;

            while (dealerCardTotal < 17)
            {
                if ((userCardTotal >= 21) || (surrender))
                    break;

                Hit();
                dealerHand.Add(currentCard);
                int currentCardPoints = CardWorth(currentCard);
                dealerCardTotal += currentCardPoints;
                image = AssignFaceValue();
                dealerPoints.Content = dealerCardTotal;

                if (dealerCardCount == 2)
                    dealerCard3.Source = image;
                if (dealerCardCount == 3)
                    dealerCard4.Source = image;
                if (dealerCardCount == 4)
                    dealerCard5.Source = image;
                if (dealerCardCount == 5)
                    dealerCard6.Source = image;

                dealerCardCount++;
            }
        }

        // Default hand view. Two cards to user, 
        // and two cards to dealer, the second of which
        // is hidden until user stands. 
        private void NewHand()
        {
            for (int i = 0; i < 2; i++)
            {
                Hit();
                if ((i == 0))
                    previousCard = currentCard;

                userHand.Add(currentCard);
                int currentCardPoints = CardWorth(currentCard);
                userCardTotal = userCardTotal + currentCardPoints;
                BitmapImage image = AssignFaceValue();
                userPoints.Content = userCardTotal;
                if (i == 0)
                    userCard1.Source = image;
                if (i == 1)
                {
                    userCard2.Source = image;
                    splitCardTotal = CardWorth(currentCard);
                    if (CardWorth(previousCard) == CardWorth(currentCard))
                        SplitButton.Visibility = System.Windows.Visibility.Visible;
                }

                Hit();
                dealerHand.Add(currentCard);
                currentCardPoints = CardWorth(currentCard);
                image = AssignFaceValue();
                if (i == 0)
                {
                    dealerCardTotal = dealerCardTotal + currentCardPoints;
                    dealerPoints.Content = dealerCardTotal;
                    dealerCard1.Source = image;
                }
                if (i == 1)
                {
                    dealerSecondCard = currentCard;
                }
            }
            if (dealerCardTotal == 11)
                SurrenederButton.Visibility = Visibility.Visible;
            if ((userCardTotal == 21) && (userCardCount == 2))
            {
                blackjackHand = true;
                Stand();
            }

        }

        // Moves the second card to the split location
        // and handles the addition of the new cards to 
        // both the split and main. Also gives the dealer
        // one card and stores the other to return on stand
        private void SplitHand()
        {
            splitCard1.Source = userCard2.Source;
            userCard2.Source = null;

            //splitCardTotal = userCardTotal - CardWorth(previousCard);

            userCardTotal = CardWorth(previousCard);
            // splitCardTotal = CardWorth(currentCard);
            Hit();
            int currentCardPoints = CardWorth(currentCard);
            userCardTotal = userCardTotal + currentCardPoints;
            BitmapImage image = AssignFaceValue();
            userCard2.Source = image;

            Hit();
            currentCardPoints = CardWorth(currentCard);
            splitCardTotal = splitCardTotal + currentCardPoints;
            image = AssignFaceValue();
            splitCard2.Source = image;
            splitHandPoints.Content = splitCardTotal;
            userPoints.Content = userCardTotal;

        }

        // Determines the current card's, returned from
        // Hit(), value.
        private int CardWorth(int card)
        {
            if (aces.Contains(card))
            {
                if (userCardTotal >= 11)
                    cardPoints = 1;
                if (userCardTotal <= 10)
                    cardPoints = 11;

                if (dealerCardTotal > 10)
                    cardPoints = 1;
                if (dealerCardTotal <= 10)
                    cardPoints = 11;

                if (splitCardTotal > 10)
                    cardPoints = 1;
                if (splitCardTotal <= 10)
                    cardPoints = 11;
            }
            if (twos.Contains(card))
                cardPoints = 2;
            if (threes.Contains(card))
                cardPoints = 3;
            if (fours.Contains(card))
                cardPoints = 4;
            if (fives.Contains(card))
                cardPoints = 5;
            if (sixes.Contains(card))
                cardPoints = 6;
            if (sevens.Contains(card))
                cardPoints = 7;
            if (eights.Contains(card))
                cardPoints = 8;
            if (nines.Contains(card))
                cardPoints = 9;
            if (tens.Contains(card) || jacks.Contains(card) || queens.Contains(card) || kings.Contains(card))
                cardPoints = 10;
            return cardPoints;
        }

        // Handles the announcemnets of the main hand
        // as well as the booleans needed to 
        // handle the result.
        private String BetStatus()
        {
            String betStatus = "";

            if ((userCardTotal > dealerCardTotal) && (dealerCardTotal <= 21))
            {

                betStatus = "Congrats, you won the hand and a pot of : $" + userBet.Content.ToString();
                goodSurrender = false;
                win = true;
                push = false;
            }
            if (userCardTotal > 21)
            {
                betStatus = "You have gone bust, losing the pot of : $" + userBet.Content.ToString();
                goodSurrender = false;
                win = false;
                push = false;
            }
            if ((userCardTotal < 21) && (dealerCardTotal > 21))
            {
                betStatus = "The dealer has bust, winning you the pot of : $" + userBet.Content.ToString();
                goodSurrender = false;
                win = true;
                push = false;
            }
            if ((userCardTotal < dealerCardTotal) && (dealerCardTotal <= 21))
            {
                betStatus = "You have lost the hand and a pot of : $" + userBet.Content.ToString();
                goodSurrender = false;
                win = false;
                push = false;
            }
            if ((userCardTotal == dealerCardTotal) && (userCardTotal <= 21))
            {
                betStatus = "Push, you have tied the hand.";
                goodSurrender = false;
                push = true;
                win = false;
            }
            if ((userCardTotal == 21))
            {
                betStatus = "Nice Black Jack";
                if (userCardTotal > dealerCardTotal)
                {
                    betStatus = "Nice Black Jack Win.";
                    goodSurrender = false;
                    win = true;
                    push = false;
                }
                if (userCardTotal == dealerCardTotal)
                {
                    betStatus = "What luck, two Black Jacks. Hand is pushed.";
                    goodSurrender = false;
                    push = true;
                    win = false;
                }
            }
            if (surrender)
            {
                if (dealerCardTotal == 21)
                {
                    goodSurrender = true;
                    win = false;
                    push = false;
                    betStatus = "Nice choice, the dealer had black jack. You recieve half your bet back.";
                }
                else
                {
                    goodSurrender = false;
                    win = false;
                    push = false;
                    betStatus = "Bad call. Should have played it out.";
                }
            }
            return betStatus;
        }

        // Handles the announcments of the split hand
        // as well as the booleans needed to 
        // handle the result.
        private String SplitBetStatus()
        {
            String SplitbetStatus = "";

            if ((splitCardTotal > dealerCardTotal) && (dealerCardTotal <= 21))
            {

                SplitbetStatus = "Congrats, you won the split hand and a pot of : $" + userBet.Content.ToString();
                splitWin = true;
                splitPush = false;
            }
            if (splitCardTotal > 21)
            {
                SplitbetStatus = "You have gone bust, losing the pot of : $" + userBet.Content.ToString();
                splitWin = false;
                splitPush = false;
            }
            if ((splitCardTotal < 21) && (dealerCardTotal > 21))
            {
                SplitbetStatus = "The dealer has bust, winning you the pot of : $" + userBet.Content.ToString();
                splitWin = true;
                splitPush = false;
            }
            if ((splitCardTotal < dealerCardTotal) && (dealerCardTotal <= 21))
            {
                SplitbetStatus = "You have lost the split hand and a pot of : $" + userBet.Content.ToString();
                splitWin = false;
                splitPush = false;
            }
            if ((splitCardTotal == dealerCardTotal) && (userCardTotal <= 21))
            {
                SplitbetStatus = "Push, you have tied the split hand.";
                splitPush = true;
                splitWin = false;
            }
            if ((splitCardTotal == 21))
            {
                SplitbetStatus = "Nice Black Jack";
                if (splitCardTotal > dealerCardTotal)
                {
                    SplitbetStatus = "Nice Black Jack Win.";
                    splitWin = true;
                    splitPush = false;
                }
                if (splitCardTotal == dealerCardTotal)
                {
                    SplitbetStatus = "What luck, two Black Jacks. Split Hand is pushed.";
                    splitPush = true;
                    splitWin = false;
                }
            }


            return SplitbetStatus;
        }

        // Used to generate a new shuffled deck
        // and a new to deck to shuffle for the next
        // deck.Shuffle() call.
        private void NewDeck()
        {
            deck.Shuffle();
            ShuffledDeck = new List<int>(deck);
            deck = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9,
                                  10, 11, 12, 13, 14, 15, 16,
                                  17, 18, 19, 20, 21, 22, 23,
                                  24, 25, 26, 27, 28, 29, 30,
                                  31, 32, 34, 35, 36, 37, 38,
                                  39, 40, 41, 42, 43, 44, 45,
                                  46, 47, 48, 49, 50, 51, 52 };
        }

        // Function used to set all containers
        // and boolean values to their default
        // state.
        private void ResetFelt()
        {
            Place1.IsEnabled = true;
            Place10.IsEnabled = true;
            Place100.IsEnabled = true;
            Place500.IsEnabled = true;

            userCard1.Source = null;
            userCard2.Source = null;
            userCard3.Source = null;
            userCard4.Source = null;
            userCard5.Source = null;
            userCard6.Source = null;

            dealerCard1.Source = null;
            dealerCard2.Source = null;
            dealerCard3.Source = null;
            dealerCard4.Source = null;
            dealerCard5.Source = null;
            dealerCard6.Source = null;

            win = false;
            push = false;
            split = false;
            splitWin = false;
            splitPush = false;
            surrender = false;
            blackjackHand = false;
            splitHandBlackjack = false;

            splitCard1.Source = null;
            splitCard2.Source = null;
            splitCard3.Source = null;
            splitCard4.Source = null;
            splitCard5.Source = null;
            splitCard6.Source = null;

            HitButton.IsEnabled = true;

            userTagSplit.Content = "";
            splitHandPoints.Content = "";
            splitBetStatus.Content = "";
            DeckStatus.Content = "";

            SplitButton.Visibility = System.Windows.Visibility.Hidden;
            SplitHitButton.Visibility = System.Windows.Visibility.Hidden;

            betStatusLabel.Content = "";
            splitBetStatus.Content = "";

            SurrenederButton.Visibility = Visibility.Hidden;
            DoubleDownButton.Visibility = Visibility.Hidden;

            currentBet = 0;

            userCardCount = 2;
            dealerCardCount = 2;
            splitCardCount = 2;
            place1 = 0;
            place10 = 0;
            place100 = 0;
            place500 = 0;
            betTotal = 0;
            currentBet = 0;
            userCardTotal = 0;
            userPoints.Content = userCardTotal;
            dealerCardTotal = 0;
            dealerPoints.Content = dealerCardTotal;
        }

        // The state of containers in the view
        // after a bet has been placed.
        private void PreBetFelt()
        {
            HitButton.IsEnabled = false;
            StandButton.IsEnabled = false;
            NextHand.IsEnabled = false;
            DoubleDownButton.Visibility = System.Windows.Visibility.Hidden;
            SplitButton.Visibility = System.Windows.Visibility.Hidden;
            InsuranceButton.Visibility = System.Windows.Visibility.Hidden;
            SurrenederButton.Visibility = System.Windows.Visibility.Hidden;
        }

        // The state of containers in the view
        // before a bet has been placed.
        private void PostBetFelt()
        {
            HitButton.IsEnabled = true;
            StandButton.IsEnabled = true;
            Place10.IsEnabled = false;
            Place100.IsEnabled = false;
            Place1.IsEnabled = false;
            Place500.IsEnabled = false;
            DoubleDownButton.Visibility = Visibility.Visible;
        }

        // Function used when the Double Down
        // Option is selected.
        public void DoubleDown()
        {
            HitButton.IsEnabled = false;
            StandButton.IsEnabled = false;
            doubleDownBet = currentBet * 2;
            userBet.Content = doubleDownBet;
            differenceBet = doubleDownBet - currentBet;
            playerTotal = playerTotal - differenceBet;
            userBalance.Content = playerTotal;

            Hit();
            userHand.Add(currentCard);
            userCardTotal += CardWorth(currentCard);
            userPoints.Content = userCardTotal;
            BitmapImage image = AssignFaceValue();
            userCard3.Source = image;
            Stand();
            if (win)
                playerTotal = playerTotal + (currentBet * 2);
            if (push)
                playerTotal = playerTotal + currentBet;
        }

        // Function used to handle the
        // surrender case
        private void Surrender()
        {
            betStatusLabel.Content = BetStatus();
        }

        // Checks the CSV formatted file
        // to see if the entered used name 
        // exists, if it does, reads in the
        // balance associated with that name
        // and sets variables to reflect the login
        private void UserLogin()
        {
            matchList = new List<string> { };
            newUserLine = "";
            FileGetter();
            userLoggedIn = userLoginBox.Text;
            Regex regex = new Regex(userLoggedIn + @",\d+", RegexOptions.IgnoreCase);
            while (!StreamReader.EndOfStream)
            {
                line = StreamReader.ReadLine();
                String line1 = line.Trim();
                Match match = regex.Match(line1);
                if (match.Success)
                {
                    String lineToedit = line;
                    lineToedit = lineToedit.Replace(userLoggedIn + ",", "");
                    playerTotal = Int32.Parse(lineToedit);
                    userBalance.Content = playerTotal;
                    newUserLogged = false;
                }
                matchList.Add(match.Success.ToString());
                
            }
            if (!matchList.Contains("True"))
                newUserLogged = true;
            if(newUserLogged)
            {
                newUserLine = userLoggedIn + "," + playerTotal.ToString();
            }
            file.Close();
        }

        // Saves the balance that is 
        // currently being displayed in 
        // the player balance label into the
        // CSV file where it belongs
        private void SaveBalance()
        {
            FileGetter();
            lines = new List<string> { };
            Regex regex = new Regex(userLoggedIn + @",\d+");            
            while (!StreamReader.EndOfStream)
            {
                line = StreamReader.ReadLine();
                Match match = regex.Match(line);
                if (match.Success)
                {
                    String lineToedit = match.Value;
                    Regex regex1 = new Regex(@"\d+");
                    String playerBetTotal = playerTotal.ToString();
                    line = regex1.Replace(lineToedit, playerBetTotal);
                    
                }
                lines.Add(line);
            }
            file.Close();
            if (newUserLogged)
                lines.Add(newUserLine);
            newUserLogged = false;

            File.WriteAllLines(filePath, lines);
            lines = new List<string> { };
        }

        // used to open a file
        private void FileGetter()
        {
            String path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            path = path.Replace("file:\\", "");
            path = path + "\\UserLog.csv";
            filePath = path;
            file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            StreamReader = new StreamReader(file);
            StreamWriter = new StreamWriter(file);
        }
    }
    
    // shuffles a new deck 
    static class MyExtensions
    {
        private static Random rng = new Random();
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}