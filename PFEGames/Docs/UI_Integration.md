# Gobflin — Guide d'intégration de l'UI

Comment monter chaque écran dans Unity, brancher les références, et tout ranger en prefabs.
Scripts : `Assets/Project/Scripts/Core/UI/`

---

## 1. Règles communes (valables pour TOUS les écrans)

1. **Un écran = un panneau** enfant du Canvas, avec un script qui hérite de `UIScreen`.
2. **Le panneau reste ACTIF** dans la hiérarchie. Ne jamais le désactiver (case cochée en haut de l'Inspector) : c'est le `CanvasGroup` (alpha 0) qui le cache.
3. Le **CanvasGroup s'ajoute tout seul** quand on pose un script `UIScreen` (grâce à `RequireComponent`). Ne pas le supprimer.
4. **RectTransform du panneau** : ancres *stretch / stretch* (Alt + clic sur le preset en bas à droite), Left/Right/Top/Bottom = 0 → plein écran.
5. **Ordre dans la hiérarchie** = ordre d'affichage. Ce qui est **en bas** s'affiche **par-dessus**. Le HUD en haut, le `ConfirmPopup` tout en bas.
6. **Chaque bouton** reçoit `UIButtonFX` (ajoute l'`Outline` tout seul).
7. **Textes non cliquables** (titres, descriptions) : décocher `Raycast Target` pour ne pas bloquer les clics.
8. **Fond des panneaux** : une `Image` noire alpha ~200 en premier enfant, pour cacher ce qu'il y a derrière.

---

## 2. Canvas de base (une fois par scène)

1. *GameObject → UI → Canvas*.
2. **Canvas** : `Render Mode = Screen Space - Overlay`.
3. **Canvas Scaler** : `UI Scale Mode = Scale With Screen Size`, `Reference Resolution = 1920 × 1080`, `Match = 0.5`.
4. **EventSystem** (créé avec le Canvas) : si Unity affiche un message jaune, cliquer **« Replace with InputSystemUIInputModule »** (le projet utilise le New Input System).
5. Créer un objet vide **`UIManager`** (enfant de la racine UI, pas du Canvas) → script `UIManager`.

| Champ `UIManager` | Scène de jeu | MenuScene |
|---|---|---|
| Pause Screen | `PausePanel` | *(vide)* |
| Is Gameplay Scene | ✅ coché | ❌ décoché |

---

## 3. Les écrans

### 3.1 ConfirmPopup — « Êtes-vous sûr ? » (mockup 8)

```
ConfirmPopup        ← ConfirmPopup.cs
├─ Background       (Image noire alpha 200, stretch)
└─ Box              (Image, ~600×300, centrée)
   ├─ TxtMessage    (TMP, centré)
   ├─ BtnConfirm    (Button + UIButtonFX, texte "Quitter")
   └─ BtnCancel     (Button + UIButtonFX, texte "Annuler")
```
| Champ | Glisser |
|---|---|
| Txt Message | `TxtMessage` |
| Btn Confirm | `BtnConfirm` |
| Btn Cancel | `BtnCancel` |

- Doit être **le dernier enfant du Canvas** (par-dessus tout).
- Le texte est remplacé par le code (`Open("Quitter la partie ?", ...)`), mettre n'importe quoi.

**Test :** Pause → Quitter → le pop-up apparaît → Annuler revient à la pause.

---

### 3.2 PausePanel + Carnet de quêtes (mockup 5)

```
PausePanel                ← PauseScreen.cs
├─ Background
├─ Buttons                (Vertical Layout Group, à gauche)
│  ├─ BtnResume           (Button + UIButtonFX)
│  ├─ BtnOptions          (Button + UIButtonFX)
│  └─ BtnQuit             (Button + UIButtonFX)
└─ QuestJournal           ← QuestJournal.cs (à droite)
   ├─ Title               (TMP "CARNET DE QUÊTES")
   ├─ List                (Vertical Layout Group, Spacing 10, Child Force Expand Width ✅)
   │  └─ QuestButtonTemplate  (Button + UIButtonFX, avec un TMP enfant)
   └─ EmptyText           (TMP "Aucune quête pour l'instant")
```
| Champ `PauseScreen` | Glisser |
|---|---|
| Btn Resume / Btn Options / Btn Quit | les 3 boutons |
| Options Screen | `OptionsPanel` |
| Confirm Popup | `ConfirmPopup` |
| Menu Scene | `MenuScene` |
| Quest Journal | l'objet `QuestJournal` |

| Champ `QuestJournal` | Glisser |
|---|---|
| List Parent | `List` |
| Quest Button Prefab | `QuestButtonTemplate` (le bouton **dans** la liste, il se cache tout seul) |
| Empty Text | `EmptyText` |
| Detail Screen | `QuestDetailPanel` |

**Test :** Échap → pause (jeu figé, souris visible). Sans quête → « Aucune quête ». Accepter une quête → elle apparaît.

---

### 3.3 QuestDetailPanel — détail d'une quête

```
QuestDetailPanel     ← QuestDetailScreen.cs
├─ Background
└─ Box
   ├─ TxtTitle       (TMP, gras)
   ├─ TxtDescription (TMP)
   ├─ TxtMissions    (TMP, Rich Text ✅ pour le texte rayé)
   └─ BtnClose       (Button "✕" + UIButtonFX)
```
| Champ | Glisser |
|---|---|
| Txt Title / Txt Description / Txt Missions | les 3 textes |
| Btn Close | `BtnClose` |

- Placer **sous** `PausePanel` dans la hiérarchie.

**Test :** Pause → clic sur une quête → détail. Mission faite = rayée. ✕ ou Échap → retour pause.

---

### 3.4 OptionsPanel (mockups 3 et 6)

```
OptionsPanel         ← UIOption.cs
├─ Background
├─ Title             (TMP "Options")
├─ SliderMusic       (Slider, Min 0, Max 1, Value 1) + label "Musique"
├─ SliderSFX         (Slider, Min 0, Max 1, Value 1) + label "Effets"
├─ ToggleFullscreen  (Toggle) + label "Plein écran"
├─ DropdownQuality   (UI → Dropdown - TextMeshPro) + label "Qualité"
├─ BtnControls       (Button + UIButtonFX "Contrôles")
└─ BtnBack           (Button + UIButtonFX "Retour")
```
| Champ | Glisser |
|---|---|
| Slider Music / Slider SFX | les 2 sliders |
| Full Screen | le Toggle |
| Btn Back | `BtnBack` |
| Dropdown Quality | le Dropdown |
| Btn Controls | `BtnControls` |
| Controls Screen | `ControlsPanel` |

- ⚠️ Le **SoundManager** doit être dans la scène (sinon erreur au `Start`).
- Les options du Dropdown sont remplies par le code (niveaux de *Project Settings → Quality*).
- Le plein écran ne fait rien dans l'éditeur, seulement en build.

**Test :** Pause → Options → bouger les sliders (le son change) → relancer le jeu → les valeurs sont gardées.

---

### 3.5 ControlsPanel + rebind (mockups 3 et 6)

```
ControlsPanel            ← ControlsScreen.cs
├─ Background
├─ Title                 (TMP "Contrôles")
├─ Rows                  (Vertical Layout Group)
│  └─ Row_Jump           (Horizontal Layout Group)
│     ├─ Label           (TMP "Sauter")
│     └─ BtnKey          (Button + UIButtonFX + RebindButton.cs)
│        └─ TxtKey       (TMP)
├─ BtnReset              (Button + UIButtonFX "Réinitialiser")
└─ BtnBack               (Button + UIButtonFX "Retour")
```
| Champ `ControlsScreen` | Glisser |
|---|---|
| Btn Back / Btn Reset | les 2 boutons |
| Input Reader | l'asset **InputReader** (le même que celui du joueur) |

Une ligne par action — dupliquer `Row_Jump` et changer `RebindButton` :

| Label | Action Name | Part Name |
|---|---|---|
| Avancer | `Move` | `up` |
| Reculer | `Move` | `down` |
| Gauche | `Move` | `left` |
| Droite | `Move` | `right` |
| Sauter | `Jump` | *(vide)* |
| S'accroupir | `Crouch` | *(vide)* |
| Courir | `Sprint` | *(vide)* |
| Interagir / Grimper | `Interact` | *(vide)* |
| Pousser / tirer | `Grab` | *(vide)* |
| Lancer | `ThrowObject` | *(vide)* |

| Champ `RebindButton` | Valeur |
|---|---|
| Input Reader | l'asset **InputReader** |
| Action Name | voir tableau (respecter majuscules) |
| Part Name | voir tableau |
| Txt Key | le `TxtKey` enfant du bouton |

- ❌ Ne pas faire de ligne pour **Aim** (molette = axe) ni **Climb** (l'escalade utilise `Interact` dans le code).

**Test :** clic sur Sauter → « Appuie sur une touche… » → F → le gobelin saute avec F. Échap pendant l'attente = annule (l'écran reste ouvert). Réinitialiser → retour à Espace.

---

### 3.6 VictoryPanel (mockup 7)

```
VictoryPanel        ← VictoryScreen.cs
├─ Background
├─ Title            (TMP "BRAVO JEUNE GOBELIN, TU AS RÉUSSI TA MISSION", pivot au centre)
└─ BtnMenu          (Button + UIButtonFX "Retour au menu principal")
```
| Champ | Glisser |
|---|---|
| Btn Menu | `BtnMenu` |
| Title | `Title` (son Transform) |
| Menu Scene | `MenuScene` |

**Zone de fin (dans le niveau, pas dans l'UI) :**
1. Objet vide `VictoryZone` à l'endroit de l'artefact.
2. `Box Collider` → **Is Trigger ✅**.
3. Script `VictoryTrigger` → **Victory Screen** = `VictoryPanel`.

**Test :** entrer dans la zone → écran de victoire. Échap ne ferme pas, Espace / Échap / clic → menu.

---

### 3.7 HUD — Pastille E (mockup 4)

```
HUD
└─ InteractPrompt   ← InteractPrompt.cs (~50×50, ancre + pivot au CENTRE)
   ├─ Image ronde
   └─ TxtKey        (TMP "E", centré)
```
| Champ | Glisser |
|---|---|
| Player | le **PlayerManager** du gobelin (dans la scène) |
| Input Reader | l'asset **InputReader** |
| Txt Key | `TxtKey` |

**Test :** s'approcher d'un levier, d'un objet, d'un mur grimpable, d'une caisse → la pastille flotte au-dessus.

---

### 3.8 HUD — Jauge de puissance (mockup 4)

```
HUD
└─ PowerGauge       ← PowerGauge.cs (le CanvasGroup s'ajoute tout seul)
   ├─ Background    (Image, ~30×150, sprite à graduations si possible)
   └─ Fill          (Image : Image Type = Filled, Fill Method = Vertical, Fill Origin = Bottom)
```
| Champ | Glisser / régler |
|---|---|
| Player | le **PlayerManager** du gobelin |
| Fill | `Fill` |
| Colors | Gradient vert → jaune → rouge (⚠️ par défaut il est blanc) |
| Screen Offset | (60, 0) = à droite du gobelin |

**Test :** ramasser un objet → la jauge apparaît. Molette → elle monte/descend par crans.

---

### 3.9 HUD — Notification de quête (mockup 4)

```
HUD
└─ QuestNotification   ← QuestNotification.cs (en haut au centre, ~700×80)
   ├─ Background       (Image, alpha léger)
   └─ TxtMessage       (TMP centré)
```
| Champ | Valeur |
|---|---|
| Txt Message | `TxtMessage` |
| Fade Duration | 0.3 (par défaut) |
| Display Duration | 2.5 (par défaut) |

- Le `QuestManager` doit être dans la scène.

**Test :** accepter une quête → « Une quête a été ajoutée dans mon carnet ». Ramasser le dernier objet → « Mission accomplie » puis « Retourne voir la créature ! ».

---

### 3.10 Dialogue — gag « Refuser » (mockup 4 bis)

Rien à monter : utilise le panneau de dialogue existant (`DialogueManager`).
- **Refuse Text** = `Refuser`.
- `choicesContainer` doit avoir un **Horizontal** ou **Vertical Layout Group**.
- Dans le **graphe de dialogue**, écrire exactement `Refuser` sur le choix (pas « BobRefus »).

**Test :** parler à la créature → clic Refuser → le bouton tombe, seul Accepter reste.

---

### 3.11 Menu principal (MenuScene, mockups 1 et 2)

Garder l'objet existant avec `UIMenuStart`, et ajouter dans le Canvas du menu les prefabs **OptionsPanel**, **ControlsPanel**, **ConfirmPopup**.

| Champ `UIMenuStart` | Glisser |
|---|---|
| Logo / Text / Btn Play / Btn Settings / Btn Quit / Fade Transition | comme avant |
| Options Screen | `OptionsPanel` |
| Confirm Popup | `ConfirmPopup` |
| Game Scene | `KaciScene` (puis `FinalScene` quand le niveau est prêt) |

- `UIButtonFX` sur Jouer / Options / Quitter.
- `FadeTransition` (Image noire plein écran) : **dernier enfant du Canvas**, pour que le fondu couvre tout.
- `UIManager` avec **Is Gameplay Scene ❌**.
- **SoundManager** présent (il passera en jeu tout seul : `DontDestroyOnLoad`).

---

## 4. Tout transformer en prefabs

Dossier : `Assets/Project/Prefabs/UI/`

### 4.1 Prefabs partagés (créer d'abord)
Glisser chaque panneau de la hiérarchie vers le dossier :
- `PF_ConfirmPopup`
- `PF_OptionsPanel`
- `PF_ControlsPanel`

Ils servent dans **les deux** scènes.

### 4.2 Prefab racine du jeu : `PF_UI_Game`
Monter cette hiérarchie, puis glisser `UI_Game` dans le dossier :
```
UI_Game
├─ Canvas
│  ├─ HUD                    (objet vide stretch, en PREMIER)
│  │  ├─ InteractPrompt
│  │  ├─ PowerGauge
│  │  └─ QuestNotification
│  ├─ DialoguePanel          (celui du DialogueManager)
│  ├─ PausePanel
│  ├─ QuestDetailPanel
│  ├─ PF_OptionsPanel        (prefab imbriqué)
│  ├─ PF_ControlsPanel       (prefab imbriqué)
│  ├─ VictoryPanel
│  └─ PF_ConfirmPopup        (prefab imbriqué, en DERNIER)
├─ UIManager                 (Is Gameplay Scene ✅)
└─ EventSystem               (InputSystemUIInputModule)
```

### 4.3 Prefab racine du menu : `PF_UI_Menu`
```
UI_Menu
├─ Canvas
│  ├─ (logo, textes, boutons, UIMenuStart)
│  ├─ PF_OptionsPanel
│  ├─ PF_ControlsPanel
│  ├─ PF_ConfirmPopup
│  └─ FadeTransition         (en DERNIER : le fondu couvre tout)
├─ UIManager                 (Is Gameplay Scene ❌, Pause Screen vide)
└─ EventSystem
```

### 4.4 ⚠️ Références vers la scène
Un prefab **ne peut pas garder** une référence vers un objet de la scène. Après avoir posé `PF_UI_Game` dans une scène, re-glisser **sur l'instance** :
- `InteractPrompt` → **Player**
- `PowerGauge` → **Player**
- `VictoryTrigger` (dans le niveau) → **Victory Screen**

Les références **internes** au prefab (boutons, panneaux entre eux) et vers des **assets** (InputReader) sont gardées.

### 4.5 Modifier un prefab
- Double-clic sur le prefab dans Project → **mode Prefab** → modifier → la modif s'applique partout.
- Si on modifie une instance dans la scène : en haut de l'Inspector, **Overrides → Apply All** pour la renvoyer dans le prefab.

---

## 5. Réglages projet

1. **File → Build Profiles / Build Settings** : `MenuScene` en **index 0**, puis `FinalScene` (et `KaciScene` pour les tests).
2. **Project Settings → Quality** : ajouter des niveaux (Bas / Moyen / Élevé / Ultra) pour le Dropdown.
3. Chaque scène de jeu contient : `PF_UI_Game`, le joueur, `QuestManager`, `SoundManager`, `DialogueManager`.

---

## 6. Check-list de test finale

- [ ] Menu : Espace → logo → boutons apparaissent avec hover
- [ ] Menu : Options → Contrôles → Échap → Échap → souris toujours visible
- [ ] Menu : Quitter → Annuler / Jouer → fondu → scène de jeu
- [ ] Jeu : Échap → pause, jeu figé, souris visible ; Échap → jeu, souris bloquée
- [ ] Pause → Options → sliders / qualité / plein écran → Échap → retour pause
- [ ] Contrôles : rebind + Réinitialiser, sauvegardé après relance
- [ ] Carnet : vide → quête acceptée → mission rayée → « Missions Accomplies »
- [ ] Notifications : acceptée / mission / retourne voir / terminée
- [ ] Pastille E sur levier, objet, mur, caisse
- [ ] Jauge : apparaît en portant un objet, crans de molette
- [ ] Dialogue : Refuser tombe
- [ ] Zone de fin → victoire → menu
- [ ] Pause → Quitter → confirmer → menu qui s'anime normalement (pas figé)
