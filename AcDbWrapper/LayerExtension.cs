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

namespace AcDbWrapper
{
    public static class LayerExtension
    {
        /// <summary>
        /// Provides the layer state change options.
        /// </summary>
        public enum LayerState
        {
            Unlock = 1,
            Thaw = 2,
            TurnOn = 4,
        }

        /// <summary>
        /// Temporarily changes the layer state.
        /// </summary>
        /// <param name="layerId">The layer id.</param>
        /// <param name="state">The layer state to change to.</param>
        /// <returns>A disposable object that returns the layer to it's original state when disposed.</returns>
        public static Disposable TempLayerState(ObjectId layerId, LayerState state)
        {
            if (layerId.IsMatch<LayerTableRecord>())
            {
                bool started = layerId.Database.GetOrStartTransaction(out Transaction transaction);
                using Disposable disposable = new(transaction, started);

                using LayerTableRecord layer = transaction.GetObject(layerId, OpenMode.ForRead) as LayerTableRecord;

                bool locked = layer.IsLocked;
                bool off = layer.IsOff;
                bool frozen = layer.IsFrozen;

                if (state.HasFlag(LayerState.Unlock).Equals(locked)
                    || state.HasFlag(LayerState.TurnOn).Equals(off)
                    || state.HasFlag(LayerState.Thaw).Equals(frozen))
                {
                    layer.UpgradeOpen();

                    if (state.HasFlag(LayerState.Unlock).Equals(locked))
                    {
                        layer.IsLocked = false;
                    }
                    if (state.HasFlag(LayerState.TurnOn).Equals(off))
                    {
                        layer.IsOff = false;
                    }
                    if (state.HasFlag(LayerState.Thaw).Equals(frozen))
                    {
                        layer.IsFrozen = false;
                    }

                    return new(() => {
                        if (layerId.IsMatch<LayerTableRecord>())
                        {
                            bool started = layerId.Database.GetOrStartTransaction(out Transaction transaction);
                            using Disposable disposable = new(transaction, started);

                            using LayerTableRecord layer = transaction.GetObject(layerId, OpenMode.ForWrite) as LayerTableRecord;

                            if (state.HasFlag(LayerState.Unlock).Equals(locked))
                            {
                                layer.IsLocked = true;
                            }
                            if (state.HasFlag(LayerState.TurnOn).Equals(off))
                            {
                                layer.IsOff = true;
                            }
                            if (state.HasFlag(LayerState.Thaw).Equals(frozen))
                            {
                                layer.IsFrozen = true;
                            }
                        }
                    });
                }
            }

            return new(() => { });
        }
    }
}
