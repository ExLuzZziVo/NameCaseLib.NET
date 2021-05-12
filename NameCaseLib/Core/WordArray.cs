#region

using System;
using NameCaseLib.NCL;

#endregion

namespace NameCaseLib.Core
{
    /// <summary>
    ///     Класс для создания динамического массива слов
    /// </summary>
    public class WordArray
    {
        private int capacity = 4;

        private Word[] words;

        /// <summary>
        ///     Создаем новый массив слов со стандартной длиной
        /// </summary>
        public WordArray()
        {
            words = new Word[capacity];
        }

        /// <summary>
        ///     Вовращает количество слов в массиве
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        ///     Получаем из массива слов слово с указаным индексом
        /// </summary>
        /// <param name="id">Индекс слова</param>
        /// <returns>Слово</returns>
        public Word GetWord(int id)
        {
            return words[id];
        }

        private void EnlargeArray()
        {
            var tmp = new Word[capacity * 2];
            Array.Copy(words, tmp, Length);
            words = tmp;
            capacity *= 2;
        }

        /// <summary>
        ///     Добавляем в массив слов новое слово
        /// </summary>
        /// <param name="word">Слово</param>
        public void AddWord(Word word)
        {
            if (Length >= capacity)
            {
                EnlargeArray();
            }

            words[Length] = word;
            Length++;
        }

        /// <summary>
        ///     Находит имя/фамилию/отчество среди слов в массиве
        /// </summary>
        /// <param name="namePart">Что нужно найти</param>
        /// <returns>Слово</returns>
        public Word GetByNamePart(NamePart namePart)
        {
            for (var i = 0; i < Length; i++)
            {
                if (words[i].NamePart == namePart)
                {
                    return words[i];
                }
            }

            return new Word("");
        }
    }
}