/*This is free and unencumbered software released into the public domain.
*
*Anyone is free to copy, modify, publish, use, compile, sell, or
*distribute this software, either in source code form or as a compiled
*binary, for any purpose, commercial or non-commercial, and by any
*means.
*
*In jurisdictions that recognize copyright laws, the author or authors
*of this software dedicate any and all copyright interest in the
*software to the public domain. We make this dedication for the benefit
*of the public at large and to the detriment of our heirs and
*successors. We intend this dedication to be an overt act of
*relinquishment in perpetuity of all present and future rights to this
*software under copyright law.
*
*THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
*EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
*MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
*IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
*OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
*ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
*OTHER DEALINGS IN THE SOFTWARE.
*
*For more information, please refer to <https://unlicense.org>
*/

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace ACADE_Utilities
{
	/// <summary>
	/// Represents a visual circuit for insertion.
	/// </summary>
	public class CircuitJig : EntityJig
	{
		#region Fields

		private Point3d dragPoint;
		private readonly AeCircuit circuit;
		private ObjectId spaceId;

		#endregion

		#region Constructors

		/// <summary>
		/// Initialize a new instance of this class.
		/// </summary>
		/// <param name="circuit"></param>
		public CircuitJig(AeCircuit circuit) : base(circuit.BlockReference)
		{
			this.circuit = circuit;
			spaceId = Application.DocumentManager.MdiActiveDocument.Database.CurrentSpaceId;
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Controls the sampling of user input.
		/// </summary>
		/// <param name="prompts"></param>
		/// <returns></returns>
		protected override SamplerStatus Sampler(JigPrompts prompts)
		{
			var options = new JigPromptPointOptions("\nSpecify insertion point: ")
			{
				UserInputControls = UserInputControls.Accept3dCoordinates
			};
			var result = prompts.AcquirePoint(options);
			if (result.Value.IsEqualTo(dragPoint))
				return SamplerStatus.NoChange;
			dragPoint = result.Value;
			return SamplerStatus.OK;
		}

		/// <summary>
		/// Update the circuit view.
		/// </summary>
		/// <returns>Always returns <c>true</c>.</returns>
		protected override bool Update()
		{
			circuit.BlockReference.Position = dragPoint;

			try
			{
				Database database = spaceId.Database;
				Transaction transaction = database.TransactionManager.StartTransaction();

				AeDrawing drawing = AeDrawing.GetOrCreate(transaction, database);
				circuit.UpdateWireNo(spaceId, drawing.AeLadders);
			}
			catch (System.Exception)
			{

				throw;
			}

			return true;
		}

		#endregion
	}
}
