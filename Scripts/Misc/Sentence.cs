using UnityEngine;

namespace KulibinSpace.DialogSystem {

    [System.Serializable]
    public struct Sentence {
        public string characterName;
        public string text;
        public Sprite characterSprite;

        public Sentence (string characterName, string text) {
            characterSprite = null;
            this.characterName = characterName;
            this.text = text;
        }
    }
}
