using YMatchThree.Core;
using YMatchThree.Editor;
using Yurowm;

namespace BubbleShooter {
    public class ColoredSelectionEditor : ContentSelectionEditor<IColored> {
        ColoredEditor coloredEditor = new ColoredEditor();
            
        public override void OnGUI(ContentInfo[] selection, LevelFieldEditor fieldEditor) {
            var colorSettings = (fieldEditor.context as LevelLayoutEditor)?.script.colorSettings;
                
            EUtils.DrawMixedProperty(selection,
                mask: c => c.Reference is IColored,
                getValue: c => c.GetVariable<ColoredVariable>().info,
                setValue: (c, value) => c.GetVariable<ColoredVariable>().info = value,
                drawSingle: (c, value) => coloredEditor.DrawSingle(colorSettings, value));
        }
    }
}
