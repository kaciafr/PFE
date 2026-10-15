using UnityEngine;

namespace Goblfin
{
    public class DialogueManager : MonoBehaviour
    {
        public static bool IsOpen => instance != null && instance.open;

        
        public static DialogueManager instance;
        public DialogueData DialogueData;
       [SerializeField] private GameObject PanelUI;
       [SerializeField]private TextMesh speakerText; 
       [SerializeField]private TextMesh bodytext;
       [SerializeField]private GameObject ContainerButton;
       public float speedtext; 
       public DialogueData  dialogue;
       public DialogueLine  lines;
       private bool open , isTyping ;

       void Awake()
        {
            if (instance == null)
            {
                instance = this; 
            }
            PanelUI.SetActive(false);
        }
        
        void Start()
        {
        
        }

        void Update()
        {
        
        }



        public void StartDialogue( int index )
        {
            if (dialogue == null || index < 0 || index >= dialogue.lines.Length)
            {
                HideDialogue();
                return;
            }

            lines = dialogue.lines[index];
            speakerText.text = lines.speaker;
            bodytext.text = lines.text;
        }

        public void EndDialogue()
        {
            
        }

        public void ShowDialogue(DialogueData d)
        {
            if (open || d == null || d.lines.Length == 0) return;

            open = true;
            dialogue = d;
            PanelUI.SetActive(true);
            StartDialogue(0);
        }

        public void HideDialogue()
        {
            open = false;
            PanelUI.SetActive(false);
        }
        
    }
}
