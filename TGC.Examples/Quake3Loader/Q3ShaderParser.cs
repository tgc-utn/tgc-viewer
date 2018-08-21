using Microsoft.DirectX.Direct3D;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using TGC.Core.Direct3D;
using TGC.Core.Textures;

namespace TGC.Examples.Quake3Loader
{
    /*
     * Clases varias para parsear los Shaders de Quake 3
     * Basado en ejemplo de Toji de Quake 3 para WebGL: http://blog.tojicode.com/2010/08/rendering-quake-3-maps-with-webgl-demo.html
     */

    /// <summary>
    ///     Herramienta para leer Tokens de shaders
    /// </summary>
    public class QShaderTokenizer
    {
        private readonly List<string> tokens = new List<string>();
        private int offset;

        public QShaderTokenizer(string code)
        {
            // elimina todos los comentarios tipo /*comentario*/ y los //comentario
            code = Regex.Replace(code, @"(/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/)|(//.*)", "");
            //genera los tokens, separa linea a linea parametro por parametro
            var match = Regex.Match(code, "[^\\s\\n\\r\"]+");

            while (match.Success)
            {
                tokens.Add(match.ToString());
                match = match.NextMatch();
            }
        }

        public bool EOF
        {
            get { return offset >= tokens.Count; }
        }

        public string GetToken()
        {
            if (offset >= 0 && offset < tokens.Count)
                return tokens[offset];

            return "";
        }

        public void MoveNext()
        {
            offset++;
        }

        public void MovePrev()
        {
            offset--;
        }

        public string GetNext()
        {
            var rta = GetToken();
            MoveNext();
            return rta;
        }
    }

    /// <summary>
    ///     Utilidades para parsear
    /// </summary>
    public static class ParserTools
    {
        public static string ToString(float f)
        {
            var _NumberFormatInfo = new NumberFormatInfo();
            _NumberFormatInfo.NumberDecimalSeparator = ".";

            return f.ToString(_NumberFormatInfo);
        }

        public static float ToFloat(string s)
        {
            var _NumberFormatInfo = new NumberFormatInfo();
            _NumberFormatInfo.NumberDecimalSeparator = ".";

            return float.Parse(s, _NumberFormatInfo);
        }
    }

    /// <summary>
    ///     Datos de Shader
    /// </summary>
    public class QShaderData
    {
        private bool builded;

        public void BuildFx()
        {
            if (builded)
                //ya fue generado el fx
                return;

            string errores;

            Fx = Effect.FromString(D3DDevice.Instance.Device, ShaderSrc, null, null, ShaderFlags.NotCloneable, null, out errores);
            //if (!errores.Equals(""))
            //errores = "";

            //grabo el source del shader a un texto
            var erroresline = "Lista de errores:\n" + errores + "\nFin Errores\n\n";
            //File.WriteAllText("shad" + Name.Replace('/', '-') + ".txt", erroresline + ShaderSrc);

            builded = true;
        }

        #region Accesors

        public string Name { get; set; }

        public string Code { get; set; }

        public string Cull { get; set; } = "";

        public bool Sky { get; set; }

        public bool Blend { get; set; }

        public bool Opaque { get; set; }

        public int Sort { get; set; }

        public List<QShaderDeform> VertexDeforms { get; set; } = new List<QShaderDeform>();

        public List<QShaderStage> Stages { get; set; } = new List<QShaderStage>();

        public string ShaderSrc { get; set; }

        public Effect Fx { get; set; }

        #endregion Accesors
    }

    /// <summary>
    ///     Datos de ShaderDeform
    /// </summary>
    public class QShaderDeform
    {
        #region Accesors

        public string Type { get; set; }

        public float Spread { get; set; }

        public float BulgeWidth { get; set; }

        public float BulgeHeight { get; set; }

        public float BulgeSpeed { get; set; }

        public QWaveForm WaveForm { get; set; }

        #endregion Accesors
    }

    /// <summary>
    ///     Datos de ShaderStage
    /// </summary>
    public class QShaderStage
    {
        //Variables extras para poder testear. Estas deberian estar en otro lado

        public bool IsLightMap()
        {
            return Map.Equals("$lightmap");
        }

        public void LoadTextures(string mediaPath)
        {
            //string root = mediaPath;
            if (!Map.Equals("anim"))
            {
                var extension_pos = Map.LastIndexOf('.');
                if (extension_pos < 0)
                    extension_pos = 0;

                var texpath = BspLoader.FindTextureExtension(Map.Substring(0, extension_pos), mediaPath);

                if (!texpath.Equals(""))
                {
                    //solo hay un mapa de bits
                    var tex = TextureLoader.FromFile(D3DDevice.Instance.Device, texpath);
                    Textures.Add(new TgcTexture(Path.GetFileName(texpath), texpath, tex, false));
                    AnimFreq = 0;
                }
            }
            else
            {
                foreach (var tex in AnimMaps)
                {
                    var extension_pos = tex.LastIndexOf('.');
                    if (extension_pos < 0)
                        extension_pos = 0;

                    var texpath = BspLoader.FindTextureExtension(tex.Substring(0, extension_pos), mediaPath);

                    if (!texpath.Equals(""))
                    {
                        //solo hay un mapa de bits
                        var t = TextureLoader.FromFile(D3DDevice.Instance.Device, texpath);
                        Textures.Add(new TgcTexture(Path.GetFileName(texpath), texpath, t, false));
                    }
                }
            }
        }

        #region Accesors

        public string Map { get; set; }

        public bool Clamp { get; set; }

        public string TcGen { get; set; } = "base";

        public string RgbGen { get; set; } = "identity";

        public QWaveForm RgbWaveform { get; set; }

        public string AlphaGen { get; set; } = "1.0";

        public string AlphaFunc { get; set; }

        public QWaveForm AlphaWaveform { get; set; }

        public string BlendSrc { get; set; } = "GL_ONE";

        public string BlendDest { get; set; } = "GL_ZERO";

        public bool HasBlendFunc { get; set; }

        public List<QTcMod> TcMods { get; set; } = new List<QTcMod>();

        public List<string> AnimMaps { get; set; } = new List<string>();

        public float AnimFreq { get; set; }

        public string DepthFunc { get; set; } = "lequal";

        public bool DepthWrite { get; set; } = true;

        public bool DepthWriteOverride { get; set; }

        public List<TgcTexture> Textures { get; set; } = new List<TgcTexture>();

        public float AnimTimeAcum { get; set; }

        #endregion Accesors
    }

    public enum QTcModType
    {
        None,
        Rotate,
        Scale,
        Scroll,
        Stretch,
        Turbulance
    }

    public class QTcMod
    {
        //private QTcModType type;

        #region Accesors

        public string Type { get; set; }

        public float Angle { get; set; }

        public float ScaleX { get; set; }

        public float ScaleY { get; set; }

        public float SSpeed { get; set; }

        public float TSpeed { get; set; }

        public QWaveForm WaveForm { get; set; }

        public QWaveForm Turbulance { get; set; }

        #endregion Accesors
    }

    public class QWaveForm
    {
        #region Accesors

        public string Name { get; set; }

        public float Bas { get; set; }

        public float Amp { get; set; }

        public float Phase { get; set; }

        public float Freq { get; set; }

        #endregion Accesors
    }

    public class StageCode
    {
        #region Accesors

        public List<string> VertexLines { get; set; } = new List<string>();

        public List<string> PixelLines { get; set; } = new List<string>();

        public List<string> StatesLines { get; set; } = new List<string>();

        #endregion Accesors
    }

    /// <summary>
    ///     Estuctura de Shader
    /// </summary>
    public class ShaderCode
    {
        public string Build()
        {
            //genero el texto Propiamente diacho
            var ShaderText = "//prueba de generacion automatica de shaders\n";

            //agrego las variables globales
            foreach (var s in Globals)
            {
                ShaderText += s + "\n";
            }

            //Espacios
            ShaderText += "\n";
            ShaderText += "\n";

            //Estructura VSInput
            ShaderText += "struct VSInput\n";
            ShaderText += "{\n";
            foreach (var s in VsStruct)
            {
                ShaderText += "\t" + s + "\n";
            }
            ShaderText += "};\n";

            //Espacios
            ShaderText += "\n";
            ShaderText += "\n";

            //Estructura PSInput
            ShaderText += "struct PSInput\n";
            ShaderText += "{\n";
            foreach (var s in PsStruct)
            {
                ShaderText += "\t" + s + "\n";
            }
            ShaderText += "};\n";

            //Espacios
            ShaderText += "\n";
            ShaderText += "\n";

            //funciones auxiliares
            foreach (var s in AuxFuntions)
            {
                ShaderText += s + "\n";
            }

            for (var i = 0; i < Stages.Count; i++)
            {
                var stage = Stages[i];

                //Espacios
                ShaderText += "\n";
                ShaderText += "\n";

                //Funcion del vertexShader
                ShaderText += "PSInput VertexShader" + i + "(VSInput In)\n";
                ShaderText += "{\n";
                foreach (var s in stage.VertexLines)
                {
                    ShaderText += "\t" + s + "\n";
                }
                ShaderText += "}\n";

                //Espacios
                ShaderText += "\n";
                ShaderText += "\n";

                //Funcion del PixelShader
                ShaderText += "float4 PixelShader" + i + "(PSInput In):COLOR\n";
                ShaderText += "{\n";
                foreach (var s in stage.PixelLines)
                {
                    ShaderText += "\t" + s + "\n";
                }
                ShaderText += "}\n";
            }

            //Espacios
            ShaderText += "\n";
            ShaderText += "\n";

            //Generacion de la tecnica
            ShaderText += "technique tec0\n";
            ShaderText += "{\n";
            for (var i = 0; i < Stages.Count; i++)
            {
                var stage = Stages[i];

                if (i > 0)
                {
                    //Espacios
                    ShaderText += "\n";
                    ShaderText += "\n";
                }

                //genero la pasada
                ShaderText += "\tpass p" + i + "\n";
                ShaderText += "\t{\n";

                //seteo el estado de la pasada
                foreach (var s in stage.StatesLines)
                {
                    ShaderText += "\t\t" + s + "\n";
                }
                ShaderText += "\t\tVertexShader = compile vs_3_0 VertexShader" + i + "();\n";
                ShaderText += "\t\tPixelShader = compile ps_3_0 PixelShader" + i + "();\n";
                ShaderText += "\t}\n";
            }
            ShaderText += "}\n";

            return ShaderText;
        }

        public StageCode NewStage()
        {
            var stage = new StageCode();
            Stages.Add(stage);
            return stage;
        }

        #region Accesors

        public List<string> Globals { get; set; } = new List<string>();

        public List<string> VsStruct { get; set; } = new List<string>();

        public List<string> PsStruct { get; set; } = new List<string>();

        public List<string> AuxFuntions { get; set; } = new List<string>();

        public List<StageCode> Stages { get; set; } = new List<StageCode>();

        #endregion Accesors
    }

    /// <summary>
    ///     Parser de Shaders
    /// </summary>
    public static class Q3ShaderParser
    {
        /// <summary>
        ///     Cargar shader desde un archivo
        /// </summary>
        public static List<QShaderData> GetFxFromFile(string file, string mediaPath)
        {
            return GetFxFromText(File.ReadAllText(file), mediaPath);
        }

        /// <summary>
        ///     Cargar shader
        /// </summary>
        public static List<QShaderData> GetFxFromText(string code, string mediaPath)
        {
            var shaders = new List<QShaderData>();
            var tokenizer = new QShaderTokenizer(code);

            // Parse a shader
            while (!tokenizer.EOF)
            {
                var shader = ParseShader(tokenizer);
                if (shader != null)
                {
                    if (shader.Stages != null)
                    {
                        // Crea un shader de directx En base a los del formato quake
                        shader.ShaderSrc = QShaderBuilderDX.BuildShaderSource(shader);
                        //shader.BuildFx();
                        foreach (var qStage in shader.Stages)
                        {
                            qStage.LoadTextures(mediaPath);
                        }
                    }
                }
                shaders.Add(shader);
            }
            return shaders;
        }

        /// <summary>
        ///     Parsear shader
        /// </summary>
        private static QShaderData ParseShader(QShaderTokenizer tokenizer)
        {
            var qsd = new QShaderData();

            qsd.Name = tokenizer.GetNext();

            var token = tokenizer.GetNext();

            if (!token.Equals("{"))
            {
                return null;
            }

            // Parse a shader
            while (!tokenizer.EOF)
            {
                token = tokenizer.GetNext().ToLower();

                if (token.Equals("}"))
                {
                    break;
                }

                switch (token)
                {
                    case "{":
                        var stage = ParseStage(tokenizer);

                        // I really really really don't like doing this, which basically just forces lightmaps to use the 'filter' blendmode
                        // but if I don't a lot of textures end up looking too bright. I'm sure I'm jsut missing something, and this shouldn't
                        // be needed.
                        if (stage.IsLightMap() && stage.HasBlendFunc)
                        {
                            stage.BlendSrc = "GL_DST_COLOR";
                            stage.BlendDest = "GL_ZERO";
                        }

                        // I'm having a ton of trouble getting lightingSpecular to work properly,
                        // so this little hack gets it looking right till I can figure out the problem
                        if (stage.AlphaGen.Equals("lightingspecular"))
                        {
                            stage.BlendSrc = "GL_ONE";
                            stage.BlendDest = "GL_ZERO";
                            stage.HasBlendFunc = false;
                            stage.DepthWrite = true;
                        }

                        if (stage.HasBlendFunc)
                        {
                            qsd.Blend = true;
                        }
                        else
                        {
                            qsd.Opaque = true;
                        }

                        qsd.Stages.Add(stage);
                        break;

                    case "cull":
                        qsd.Cull = tokenizer.GetNext();
                        break;

                    case "deformvertexes":
                        var deform = new QShaderDeform { Type = tokenizer.GetNext().ToLower() };

                        switch (deform.Type)
                        {
                            case "wave":
                                deform.Spread = 1.0f / ParserTools.ToFloat(tokenizer.GetNext());
                                deform.WaveForm = ParseWaveform(tokenizer);
                                break;

                            case "bulge":
                                deform.BulgeWidth = ParserTools.ToFloat(tokenizer.GetNext());
                                deform.BulgeHeight = ParserTools.ToFloat(tokenizer.GetNext());
                                deform.BulgeSpeed = ParserTools.ToFloat(tokenizer.GetNext());
                                break;

                            default:
                                deform = null;
                                break;
                        }

                        if (deform != null)
                        {
                            qsd.VertexDeforms.Add(deform);
                        }
                        break;

                    case "sort":
                        var sort = tokenizer.GetNext().ToLower();
                        switch (sort)
                        {
                            case "portal":
                                qsd.Sort = 1;
                                break;

                            case "sky":
                                qsd.Sort = 2;
                                break;

                            case "opaque":
                                qsd.Sort = 3;
                                break;

                            case "banner":
                                qsd.Sort = 6;
                                break;

                            case "underwater":
                                qsd.Sort = 8;
                                break;

                            case "additive":
                                qsd.Sort = 9;
                                break;

                            case "nearest":
                                qsd.Sort = 16;
                                break;

                            default:
                                qsd.Sort = int.Parse(sort);
                                break;
                        }
                        ;
                        break;

                    case "surfaceparm":
                        var param = tokenizer.GetNext().ToLower();

                        switch (param)
                        {
                            case "sky":
                                qsd.Sky = true;
                                break;

                            default:
                                break;
                        }
                        break;

                    default:
                        break;
                }
            }

            if (qsd.Sort > 0)
            {
                qsd.Sort = qsd.Opaque ? 3 : 9;
            }

            return qsd;
        }

        private static QShaderStage ParseStage(QShaderTokenizer tokenizer)
        {
            var stage = new QShaderStage();
            // Parse a Stage
            while (!tokenizer.EOF)
            {
                var token = tokenizer.GetNext();
                if (token.Equals("}"))
                {
                    break;
                }

                switch (token.ToLower())
                {
                    case "clampmap":
                        stage.Clamp = true;
                        goto case "map";

                    case "map":
                        stage.Map = tokenizer.GetNext(); //.replace()/(\.jpg|\.tga)/, '.png');
                        break;

                    case "animmap":
                        stage.Map = "anim";
                        stage.AnimFreq = ParserTools.ToFloat(tokenizer.GetNext());
                        var nextMap = tokenizer.GetNext();
                        var match = Regex.Match(nextMap, @"(\.jpg|\.tga)");
                        while (match.Success)
                        {
                            stage.AnimMaps.Add(nextMap);
                            nextMap = tokenizer.GetNext();
                            match = Regex.Match(nextMap, @"(\.jpg|\.tga)");
                        }
                        tokenizer.MovePrev();
                        break;

                    case "rgbgen":
                        stage.RgbGen = tokenizer.GetNext().ToLower();
                        switch (stage.RgbGen)
                        {
                            case "wave":
                                stage.RgbWaveform = ParseWaveform(tokenizer);
                                if (stage.RgbWaveform == null)
                                {
                                    stage.RgbGen = "identity";
                                }
                                break;
                        }
                        ;
                        break;

                    case "alphagen":
                        stage.AlphaGen = tokenizer.GetNext().ToLower();
                        switch (stage.AlphaGen)
                        {
                            case "wave":
                                stage.AlphaWaveform = ParseWaveform(tokenizer);
                                if (stage.AlphaWaveform == null)
                                {
                                    stage.AlphaGen = "1.0";
                                }
                                break;

                            default:
                                break;
                        }
                        break;

                    case "alphafunc":
                        stage.AlphaFunc = tokenizer.GetNext().ToUpper();
                        break;

                    case "blendfunc":
                        stage.BlendSrc = tokenizer.GetNext();
                        stage.HasBlendFunc = true;
                        if (stage.DepthWriteOverride)
                        {
                            stage.DepthWrite = false;
                        }
                        switch (stage.BlendSrc)
                        {
                            case "add":
                                stage.BlendSrc = "GL_ONE";
                                stage.BlendDest = "GL_ONE";
                                break;

                            case "blend":
                                stage.BlendSrc = "GL_SRC_ALPHA";
                                stage.BlendDest = "GL_ONE_MINUS_SRC_ALPHA";
                                break;

                            case "filter":
                                stage.BlendSrc = "GL_DST_COLOR";
                                stage.BlendDest = "GL_ZERO";
                                break;

                            default:
                                stage.BlendDest = tokenizer.GetNext();
                                break;
                        }
                        break;

                    case "depthfunc":
                        stage.DepthFunc = tokenizer.GetNext().ToLower();
                        break;

                    case "depthwrite":
                        stage.DepthWrite = true;
                        stage.DepthWriteOverride = true;
                        break;

                    case "tcmod":
                        var tcMod = new QTcMod
                        {
                            Type = tokenizer.GetNext().ToLower()
                        };

                        switch (tcMod.Type)
                        {
                            case "rotate":
                                tcMod.Angle = Geometry.DegreeToRadian(ParserTools.ToFloat(tokenizer.GetNext()));
                                break;

                            case "scale":
                                tcMod.ScaleX = ParserTools.ToFloat(tokenizer.GetNext());
                                tcMod.ScaleY = ParserTools.ToFloat(tokenizer.GetNext());
                                break;

                            case "scroll":
                                tcMod.SSpeed = ParserTools.ToFloat(tokenizer.GetNext());
                                tcMod.TSpeed = ParserTools.ToFloat(tokenizer.GetNext());
                                break;

                            case "stretch":
                                tcMod.WaveForm = ParseWaveform(tokenizer);
                                if (tcMod.WaveForm == null)
                                {
                                    tcMod.Type = "";
                                }
                                break;

                            case "turb":
                                tcMod.Turbulance = new QWaveForm
                                {
                                    Name = char.IsLetter(tokenizer.GetToken()[0])
                                        ? tokenizer.GetNext()
                                        : "",
                                    Bas = ParserTools.ToFloat(tokenizer.GetNext()),
                                    Amp = ParserTools.ToFloat(tokenizer.GetNext()),
                                    Phase = ParserTools.ToFloat(tokenizer.GetNext()),
                                    Freq = ParserTools.ToFloat(tokenizer.GetNext())
                                };
                                break;

                            default:
                                tcMod.Type = "";
                                break;
                        }
                        if (!tcMod.Type.Equals(""))
                        {
                            stage.TcMods.Add(tcMod);
                        }
                        break;

                    case "tcgen":
                        stage.TcGen = tokenizer.GetNext();
                        break;

                    default:
                        break;
                }
            }

            if (stage.BlendSrc.Equals("GL_ONE") && stage.BlendDest.Equals("GL_ZERO"))
            {
                stage.HasBlendFunc = false;
                stage.DepthWrite = true;
            }

            return stage;
        }

        private static QWaveForm ParseWaveform(QShaderTokenizer tokenizer)
        {
            return new QWaveForm
            {
                Name = tokenizer.GetNext().ToLower(),
                Bas = ParserTools.ToFloat(tokenizer.GetNext()),
                Amp = ParserTools.ToFloat(tokenizer.GetNext()),
                Phase = ParserTools.ToFloat(tokenizer.GetNext()),
                Freq = ParserTools.ToFloat(tokenizer.GetNext())
            };
        }
    }

    /// <summary>
    ///     Convertidor experimental de Shaders de Quake 3 a Shaders de DirectX.
    ///     No abarca todas las posibilidades.
    /// </summary>
    public static class QShaderBuilderDX
    {
        public static string BuildShaderSource(QShaderData shader)
        {
            //devuelve el codigo del Fx
            var shaderCode = new ShaderCode();

            shaderCode.AuxFuntions.Add(CreateSquareFunction());
            shaderCode.AuxFuntions.Add(CreateTriangleFunction());

            shaderCode.VsStruct.Add("float4 Pos : POSITION;");
            shaderCode.VsStruct.Add("float3 Normal : NORMAL;");
            shaderCode.VsStruct.Add("float4 Color : COLOR;");
            shaderCode.VsStruct.Add("float2 Tex0 : TEXCOORD0;");
            shaderCode.VsStruct.Add("float2 Tex1 : TEXCOORD1;");

            shaderCode.PsStruct.Add("float4 Pos : POSITION;");
            shaderCode.PsStruct.Add("float3 Normal : NORMAL;");
            shaderCode.PsStruct.Add("float4 Color : COLOR;");
            shaderCode.PsStruct.Add("float2 Tex0 : TEXCOORD0;");
            shaderCode.PsStruct.Add("float2 Tex1 : TEXCOORD1;");

            shaderCode.Globals.Add("float4x4 g_mWorld;// : WORLD;");
            shaderCode.Globals.Add("float4x4 g_mViewProj;// : VIEWPROJECTION;");
            shaderCode.Globals.Add("float g_time;// : TIME;");
            shaderCode.Globals.Add("sampler2D texture0 : register(s0);");
            shaderCode.Globals.Add("sampler2D texture1 : register(s1);");

            foreach (var qstage in shader.Stages)
            {
                var stageCode = shaderCode.NewStage();
                stageCode.VertexLines = BuildVertexShader(shader, qstage);
                stageCode.PixelLines = BuildPixelShader(shader, qstage);
                stageCode.StatesLines = BuildEffectState(shader, qstage);
            }

            return shaderCode.Build();
        }

        private static List<string> BuildEffectState(QShaderData shader, QShaderStage qstage)
        {
            var StateLines = new List<string>();

            StateLines.Add("AlphaBlendEnable = " + qstage.HasBlendFunc.ToString().ToLower() + ";");
            StateLines.Add("SrcBlend = " + GLtoDXBlend(qstage.BlendSrc) + ";");
            StateLines.Add("DestBlend = " + GLtoDXBlend(qstage.BlendDest) + ";");
            //StateLines.Add("ZWriteEnable = " + (!qstage.HasBlendFunc).ToString().ToLower() + ";");
            if (!shader.Cull.Equals(""))
            {
                if (shader.Cull.ToLower().Equals("disable"))
                {
                    shader.Cull = "None";
                }
                StateLines.Add("CullMode = " + shader.Cull + ";");
            }

            return StateLines;
        }

        private static string GLtoDXBlend(string blend)
        {
            blend = blend.ToUpper();
            string[] gl_blend =
            {
                "GL_ONE", "GL_ZERO", "GL_SRC_ALPHA", "GL_ONE_MINUS_SRC_ALPHA", "GL_DST_COLOR",
                "GL_SRC_COLOR"
            };
            string[] dx_blend = { "One", "Zero", "SrcAlpha", "InvSrcAlpha", "DestColor", "SrcColor" };

            for (var i = 0; i < gl_blend.Length; i++)
            {
                if (blend.Equals(gl_blend[i]))
                    return dx_blend[i];
            }

            //no se encontro, devuelvo cadena vacia
            return "";
        }

        private static List<string> BuildPixelShader(QShaderData shader, QShaderStage stage)
        {
            var PixelLines = new List<string>();

            PixelLines.Add("float4 texColor = tex2D(texture0, In.Tex0);");

            switch (stage.RgbGen)
            {
                case "vertex":
                    PixelLines.Add("float3 rgb = texColor.rgb * In.Color.rgb;");
                    break;

                case "wave":
                    PixelLines.Add(CreateWaveForm("rgbWave", stage.RgbWaveform, ""));
                    PixelLines.Add("float3 rgb = texColor.rgb * rgbWave;");
                    break;

                default:
                    PixelLines.Add("float3 rgb = texColor.rgb;");
                    break;
            }

            switch (stage.AlphaGen)
            {
                case "wave":
                    PixelLines.Add(CreateWaveForm("alpha", stage.AlphaWaveform, ""));
                    //PixelLines.Add("alpha = sin(g_time);");
                    break;

                case "lightingspecular":
                    // For now this is VERY special cased. May not work well with all instances of lightingSpecular
                    PixelLines.Add("float4 light = tex2D(texture1, In.Tex1);");
                    PixelLines.Add("rgb *= light.rgb;");
                    PixelLines.Add("rgb += light.rgb * texColor.a * 0.6;");
                    // This was giving me problems, so I'm ignorning an actual specular calculation for now
                    PixelLines.Add("float alpha = 1.0;");
                    break;

                default:
                    PixelLines.Add("float alpha = texColor.a;");
                    if (!stage.AlphaGen.Equals("1.0"))
                        PixelLines.Add("alpha = sin(g_time);");
                    //PixelLines.Add("float alpha = sin(g_time);");
                    break;
            }

            if (stage.AlphaFunc != null)
            {
                switch (stage.AlphaFunc)
                {
                    case "GT0":
                        PixelLines.Add("if(alpha == 0.0) { discard; }");
                        break;

                    case "LT128":
                        PixelLines.Add("if(alpha >= 0.5) { discard; }");
                        break;

                    case "GE128":
                        PixelLines.Add("if(alpha < 0.5) { discard; }");
                        break;

                    default:
                        break;
                }
            }

            //PixelLines.Add("if(rgb.r < 0.1 && rgb.g < 0.1 && rgb.b < 0.1)");
            //PixelLines.Add("alpha = 0;");
            //PixelLines.Add("discard;");

            PixelLines.Add("return float4(rgb, alpha);");

            return PixelLines;
        }

        private static List<string> BuildVertexShader(QShaderData shader, QShaderStage stage)
        {
            var VertexLines = new List<string>();

            VertexLines.Add("PSInput Out = In;");
            VertexLines.Add("float4 defPosition = In.Pos;");

            for (var i = 0; i < shader.VertexDeforms.Count; ++i)
            {
                var deform = shader.VertexDeforms[i];

                switch (deform.Type)
                {
                    case "wave":
                        {
                            var name = "deform" + i;
                            var offName = "deformOff" + i;

                            VertexLines.Add(
                                "float " + offName + " = (In.Pos.x + In.Pos.y + In.Pos.z) * " +
                                ParserTools.ToString(deform.Spread) + ";");

                            /*float phase = deform.WaveForm.Phase;
                                        //deform.WaveForm.Phase = phase.toFixed(4) + ' + ' + offName; <-----MIRAR ESTA LINEA
                                        VertexLines.Add(CreateWaveForm(name, deform.WaveForm, "g_time"));
                                        deform.WaveForm.Phase = phase;*/

                            //Parche temporal solo funciona con la funcion seno
                            VertexLines.Add("float " + name + " = " + ParserTools.ToString(deform.WaveForm.Bas) + " + sin((" +
                                            ParserTools.ToString(deform.WaveForm.Phase) + " + " +
                                            "g_time" + " * " + ParserTools.ToString(deform.WaveForm.Freq) + " + " + offName +
                                            ") * 6.283) * " + ParserTools.ToString(deform.WaveForm.Amp) + ";");
                            //FIN parche

                            VertexLines.Add("defPosition += float4(In.Normal * " + name + ",0);");
                        }
                        break;

                    case "bulge":
                        {
                            //float alpha = In.Tex0.x*bulgeWidth + g_time;
                            //float deform = sin(alpha)*bulgeHeight;
                            //defPosition += float4(In.Normal * deform0, 0);

                            var deformi = "deform" + i;
                            var alphai = "alpha" + i;

                            VertexLines.Add("float " + alphai + " = In.Tex0.x*" + deform.BulgeWidth + " + g_time*" +
                                            deform.BulgeSpeed + ";");
                            VertexLines.Add("float " + deformi + " = sin(" + alphai + ")*" + deform.BulgeHeight + ";");
                            VertexLines.Add("defPosition += float4(In.Normal * " + deformi + ",0);");
                        }
                        break;

                    default:
                        break;
                }
            }

            VertexLines.Add("float4 worldPosition = mul( defPosition, g_mWorld );");
            VertexLines.Add("Out.Color = In.Color;");

            if (stage.TcGen.Equals("environment"))
            {
                VertexLines.Add("float3 viewer = normalize(-worldPosition.xyz);");
                VertexLines.Add("float d = dot(In.Normal, viewer);");
                VertexLines.Add("float3 reflected = In.Normal*2.0*d - viewer;");
                VertexLines.Add("Out.Tex0 = float2(0.5, 0.5) + reflected.xy * 0.5;");
            }
            else
            {
                // Standard texturing
                VertexLines.Add("Out.Tex0 = In.Tex0;");
            }

            // tcMods
            for (var i = 0; i < stage.TcMods.Count; ++i)
            {
                var tcMod = stage.TcMods[i];

                switch (tcMod.Type)
                {
                    case "rotate":
                        VertexLines.Add("float r = " + ParserTools.ToString(tcMod.Angle) + " * g_time;");
                        VertexLines.Add("Out.Tex0 -= float2(0.5, 0.5);");
                        VertexLines.Add(
                            "Out.Tex0 = float2(Out.Tex0.x * cos(r) - Out.Tex0.y * sin(r), Out.Tex0.y * cos(r) + Out.Tex0.x * sin(r));");
                        VertexLines.Add("Out.Tex0 += float2(0.5, 0.5);");
                        break;

                    case "scroll":
                        VertexLines.Add(
                            "Out.Tex0 += float2(" + ParserTools.ToString(tcMod.SSpeed) + " * g_time, " +
                            ParserTools.ToString(tcMod.TSpeed) +
                            " * g_time);");
                        break;

                    case "scale":
                        VertexLines.Add(
                            "Out.Tex0 *= float2(" + ParserTools.ToString(tcMod.ScaleX) + ", " +
                            ParserTools.ToString(tcMod.ScaleY) + ");"
                            );
                        break;

                    case "stretch":
                        VertexLines.Add(CreateWaveForm("stretchWave", tcMod.WaveForm, ""));
                        VertexLines.Add("stretchWave = 1.0 / stretchWave;");
                        VertexLines.Add("Out.Tex0 *= stretchWave;");
                        VertexLines.Add("Out.Tex0 += float2(0.5 - (0.5 * stretchWave), 0.5 - (0.5 * stretchWave));");
                        break;

                    case "turb":
                        var tName = "turbTime" + i;
                        VertexLines.Add("float " + tName + " = " + ParserTools.ToString(tcMod.Turbulance.Phase) +
                                        " + g_time * " +
                                        ParserTools.ToString(tcMod.Turbulance.Freq) + ";");
                        VertexLines.Add("Out.Tex0.x += sin( ( ( In.Pos.x + In.Pos.z )* 1.0/128.0 * 0.125 + " + tName +
                                        " ) * 6.283) * " + ParserTools.ToString(tcMod.Turbulance.Amp) + ";");
                        VertexLines.Add("Out.Tex0.y += sin( ( In.Pos.y * 1.0/128.0 * 0.125 + " + tName +
                                        " ) * 6.283) * " +
                                        ParserTools.ToString(tcMod.Turbulance.Amp) + ";");
                        break;

                    default:
                        break;
                }
            }

            switch (stage.AlphaGen)
            {
                case "lightingspecular":
                    VertexLines.Add("Out.Tex1 = In.Tex1;");
                    break;

                default:
                    break;
            }

            VertexLines.Add("Out.Pos = mul(worldPosition, g_mViewProj);");
            VertexLines.Add("return Out;");

            return VertexLines;
        }

        private static string CreateTriangleFunction()
        {
            return "float triangle(float val)\n" +
                   "{ \n" +
                   "\t return abs(2.0 * frac(val) - 1.0);\n" +
                   "}";
        }

        private static string CreateSquareFunction()
        {
            return "float square(float val)\n" +
                   "{ \n" +
                   "\t return ( ((floor(val*2.0)+1.0) % 2.0) * 2.0) - 1.0;\n" +
                   "}";
        }

        public static string CreateWaveForm(string name, QWaveForm wf, string timeVar)
        {
            if (wf == null)
            {
                return "float " + name + " = 0.0;";
            }

            if (timeVar.Equals(""))
            {
                timeVar = "g_time";
            }

            var funcName = "";

            switch (wf.Name)
            {
                case "sin":
                    //retorna: float name = base + sin(( pha + t*freq )*2PI)*Amp;
                    return "float " + name + " = " + ParserTools.ToString(wf.Bas) + " + sin((" +
                           ParserTools.ToString(wf.Phase) + " + " +
                           timeVar + " * " + ParserTools.ToString(wf.Freq) + ") * 6.283) * " +
                           ParserTools.ToString(wf.Amp) + ";";

                case "square":
                    funcName = "square";
                    break;

                case "triangle":
                    funcName = "triangle";
                    break;

                case "sawtooth":
                    funcName = "frac";
                    break;

                case "inversesawtooth":
                    funcName = "1.0 - frac";
                    break;

                default:
                    return "float " + name + " = 0.0;";
            }
            //retorna: float varname = base + func(pha + t*freq)*Amp;
            return "float " + name + " = " + ParserTools.ToString(wf.Bas) + " + " + funcName + "(" +
                   ParserTools.ToString(wf.Phase) + " + " +
                   timeVar + " * " + ParserTools.ToString(wf.Freq) + ") * " + ParserTools.ToString(wf.Amp) + ";";
        }
    }
}