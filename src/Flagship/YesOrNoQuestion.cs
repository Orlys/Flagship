
namespace Flagship
{
    using System;

    public sealed class YesOrNoQuestion : YesOrNoQuestion.IYesOrNoQuestion
    {
        private string _title;
        private string _question;
        private Action _positiveAction;
        private Action _negativeAction;

        private YesOrNoQuestion(string title, string question, Action positiveAction, Action negativeAction)
        {
            this._title = title;
            this._question = question;
            this._positiveAction = positiveAction;
            this._negativeAction = negativeAction;
        }

        public bool Ask()
        {
            Console.Write($"[{ this._title }] { this._question.Trim() } ('y' for yes, 'n' for neither): ");
            c:
            var holder = Console.CursorLeft;
            var c = Console.ReadKey();
            if (c.Key == ConsoleKey.Y)
            {
                Console.CursorTop++;
                Console.CursorLeft = 0;
                this._positiveAction?.Invoke();
                return true;
            }
            else if (c.Key == ConsoleKey.N)
            {
                Console.CursorTop++;
                Console.CursorLeft = 0;
                this._negativeAction?.Invoke();
                return false;
            }
            else
            {
                if (c.Key == ConsoleKey.Escape)
                {
                    Console.Write(char.MaxValue);
                }
                else if (c.Key == ConsoleKey.Backspace)
                {
                    Console.CursorLeft++;
                }
                else if (Console.CursorLeft > 0)
                {
                    Console.CursorLeft--;
                    Console.Write(char.MinValue);
                    Console.CursorLeft--;
                }
                else if (Console.CursorLeft == 0)
                {
                    Console.CursorLeft = holder;
                    Console.Write(" ");
                    Console.CursorLeft--;
                }
                goto c;
            }
        }

        public interface IYesOrNoQuestion
        {
            bool Ask();
        }

        public interface IYesOrNoQuestionBuilder
        {
            IYesOrNoQuestionBuilder Negative(Action negativeAction);
            IYesOrNoQuestionBuilder Positive(Action positiveAction);

            IYesOrNoQuestionBuilder Title(string title);
            IYesOrNoQuestionBuilder Question(string question);

            IYesOrNoQuestion Build();
        }

        public static IYesOrNoQuestionBuilder Builder => new YesOrNoQuestionBuilder();

        private sealed class YesOrNoQuestionBuilder : IYesOrNoQuestionBuilder
        {
            internal YesOrNoQuestionBuilder()
            { 
            }

            private string _question;
            private string _title;

            private Action _negativeAction;

            public IYesOrNoQuestionBuilder Negative(Action negativeAction)
            {
                this._negativeAction = negativeAction;
                return this;
            }

            private Action _positiveAction;

            public IYesOrNoQuestionBuilder Positive(Action positiveAction)
            {
                this._positiveAction = positiveAction;
                return this;
            }

            public IYesOrNoQuestionBuilder Title(string title)
            {
                this._title = title;
                return this;
            }
            public IYesOrNoQuestionBuilder Question(string question)
            {
                this._question = question;
                return this;
            }

            public IYesOrNoQuestion Build()
            {
                return new YesOrNoQuestion(
                    this._title ?? throw new ArgumentNullException("title"),
                    this._question ?? throw new ArgumentNullException("question"),
                    this._positiveAction,
                    this._negativeAction);
            }
        }
    }
}