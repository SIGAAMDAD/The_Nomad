using Godot;

namespace Renown.Thinkers {
	public partial class Teacher : Thinker {
		[Export]
		private Resource[] Lessons;

		private int LessonProgress = 0;

		public override void Load( SaveSystem.SaveSectionReader reader, int nIndex ) {
			base.Load( reader, nIndex );

			string key = "Teacher" + nIndex;

			LessonProgress = reader.LoadInt( key + nameof( LessonProgress ) );
		}
		public override void Save( SaveSystem.SaveSectionWriter writer, int nIndex ) {
			base.Save( writer, nIndex );

			string key = "Teacher" + nIndex;

			writer.SaveInt( key + nameof( LessonProgress ), LessonProgress );
		}

		public override void _Ready() {
			base._Ready();
		}
	};
};