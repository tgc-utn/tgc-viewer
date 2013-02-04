using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.DirectX.Direct3D;
using TgcViewer;

namespace Examples.Quake3Loader
{
    /*
     * Clases varias para parsear los Shaders de Quake 3
     * Basado en ejemplo de Toji de Quake 3 para WebGL: http://blog.tojicode.com/2010/08/rendering-quake-3-maps-with-webgl-demo.html
     */

    /// <summary>
    /// Herramienta para leer Tokens de shaders
    /// </summary>
    public class QShaderTokenizer
    {
        private List<string> tokens = new List<string>();
        private int offset = 0;

        public QShaderTokenizer(string code)
        {
            // elimina todos los comentarios tipo /*comentario*/ y los //comentario
            code = Regex.Replace(code, @"(/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/)|(//.*)", "");
            //genera los tokens, separa linea a linea parametro por parametro
            Match match = Regex.Match(code, "[^\\s\\n\\r\"]+");

            while (match.Success)
            {
                tokens.Add(match.ToString());
                match = match.NextMatch();
            }
        }

        public string GetToken()
        {
            if(offset >= 0 && offset < tokens.Count)
                return tokens[offset];

            return "";
        }

        public bool EOF
        {
            get { return offset >= tokens.Count; }
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
            string rta = GetToken();
            MoveNext();
            return rta;
        }
    }

    /// <summary>
    /// Utilidades para parsear
    /// </summary>
    public static class ParserTools
    {
        public static string ToString(float f)
        {
            NumberFormatInfo _NumberFormatInfo = new NumberFormatInfo();
            _NumberFormatInfo.NumberDecimalSeparator = ".";

            return f.ToString(_NumberFormatInfo);
        }

        public static float ToFloat(string s)
        {
            NumberFormatInfo _NumberFormatInfo = new NumberFormatInfo();
            _NumberFormatInfo.NumberDecimalSeparator = ".";

            return float.Parse(s,_NumberFormatInfo);
        }
    }

    /// <summary>
    /// Datos de Shader
    /// </summary>
    public class QShaderData
    {
        private string name;
        private string code;
        private string cull;
        private bool sky;
        private bool blend;
        private bool opaque;
        private int sort;
        private List<QShaderDeform> vertexDeforms = new List<QShaderDeform>();
        private List<QShaderStage> stages = new List<QShaderStage>();
        private string shaderSrc;
        private Effect fx;

        #region Accesors
		 public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Code
        {
            get { return code; }
            set { code = value; }
        }

        public string Cull
        {
            get { return cull; }
            set { cull = value; }
        }

        public bool Sky
        {
            get { return sky; }
            set { sky = value; }
        }

        public bool Blend
        {
            get { return blend; }
            set { blend = value; }
        }

        public bool Opaque
        {
            get { return opaque; }
            set { opaque = value; }
        }

        public int Sort
        {
            get { return sort; }
            set { sort = value; }
        }

        public List<QShaderDeform> VertexDeforms
        {
            get { return vertexDeforms; }
            set { vertexDeforms = value; }
        }

        public List<QShaderStage> Stages
        {
            get { return stages; }
            set { stages = value; }
        }

        public string ShaderSrc
        {
            get { return shaderSrc; }
            set { shaderSrc = value; }
        }

        public Effect Fx
        {
            get { return fx; }
            set { fx = value; }
        }

        #endregion

        public QShaderData(){}

        public void BuildFx()
        {
            string errores;

            Fx = Effect.FromString(GuiController.Instance.D3dDevice, shaderSrc, null, null, /*ShaderFlags.NotCloneable*/ShaderFlags.None, null, out errores);
            
            if (errores != null && !errores.Equals(""))
                errores = "";

        }
        
    }

    /// <summary>
    /// Datos de ShaderDeform
    /// </summary>
    public class QShaderDeform
    {
        private string type;
        private float spread;
        private QWaveForm waveForm;

        #region Accesors
		public string Type
        {
            get { return type; }
            set { type = value; }
        }

        public float Spread
        {
            get { return spread; }
            set { spread = value; }
        }

        public QWaveForm WaveForm
        {
            get { return waveForm; }
            set { waveForm = value; }
        } 
	#endregion
    }

    /// <summary>
    /// Datos de ShaderStage
    /// </summary>
    public class QShaderStage
    {
        private string map;
        private bool clamp;
        private string tcGen = "base";
        private string rgbGen = "identity";
        private QWaveForm rgbWaveform;
        private string alphaGen = "1.0";
        private string alphaFunc;
        private QWaveForm alphaWaveform;
        private string blendSrc = "GL_ONE";
        private string blendDest = "GL_ZERO";
        private bool hasBlendFunc;
        private List<QTcMod> tcMods = new List<QTcMod>();
        private List<string> animMaps = new List<string>();
        private float animFreq = 0;
        private string depthFunc = "lequal";
        private bool depthWrite = true;
        private bool depthWriteOverride;
        //Variables extras para poder testear. Estas deberian estar en otro lado
        private List<Texture> textures=new List<Texture>();
        private float animTimeAcum;

        #region Accesors
		public string Map
        {
            get { return map; }
            set { map = value; }
        }

        public bool Clamp
        {
            get { return clamp; }
            set { clamp = value; }
        }

        public string TcGen
        {
            get { return tcGen; }
            set { tcGen = value; }
        }

        public string RgbGen
        {
            get { return rgbGen; }
            set { rgbGen = value; }
        }

        public QWaveForm RgbWaveform
        {
            get { return rgbWaveform; }
            set { rgbWaveform = value; }
        }

        public string AlphaGen
        {
            get { return alphaGen; }
            set { alphaGen = value; }
        }

        public string AlphaFunc
        {
            get { return alphaFunc; }
            set { alphaFunc = value; }
        }

        public QWaveForm AlphaWaveform
        {
            get { return alphaWaveform; }
            set { alphaWaveform = value; }
        }

        public string BlendSrc
        {
            get { return blendSrc; }
            set { blendSrc = value; }
        }

        public string BlendDest
        {
            get { return blendDest; }
            set { blendDest = value; }
        }

        public bool HasBlendFunc
        {
            get { return hasBlendFunc; }
            set { hasBlendFunc = value; }
        }

        public List<QTcMod> TcMods
        {
            get { return tcMods; }
            set { tcMods = value; }
        }

        public List<string> AnimMaps
        {
            get { return animMaps; }
            set { animMaps = value; }
        }

        public float AnimFreq
        {
            get { return animFreq; }
            set { animFreq = value; }
        }

        public string DepthFunc
        {
            get { return depthFunc; }
            set { depthFunc = value; }
        }

        public bool DepthWrite
        {
            get { return depthWrite; }
            set { depthWrite = value; }
        }

        public bool DepthWriteOverride
        {
            get { return depthWriteOverride; }
            set { depthWriteOverride = value; }
        }

        public List<Texture> Textures
        {
            get { return textures; }
            set { textures = value; }
        }

        public float AnimTimeAcum
        {
            get { return animTimeAcum; }
            set { animTimeAcum = value; }
        }

        #endregion

        public bool IsLightMap()
        {
            return Map.Equals("$lightmap");
        }

        public void LoadTextures(string mediaPath)
        {
            //string root = mediaPath;
            if(!map.Equals("anim"))
            {
                int extension_pos = map.LastIndexOf('.');
                if(extension_pos<0)
                    extension_pos = 0;

                string texpath = BspLoader.FindTextureExtension(map.Substring(0, extension_pos), mediaPath);

                if (!texpath.Equals(""))
                {
                    //solo hay un mapa de bits
                    textures.Add(TextureLoader.FromFile(GuiController.Instance.D3dDevice, texpath));
                    animFreq = 0;
                }
                
            }
            else
            {
                foreach (string tex in animMaps)
                {
                    int extension_pos = tex.LastIndexOf('.');
                    if (extension_pos < 0)
                        extension_pos = 0;

                    string texpath = BspLoader.FindTextureExtension(tex.Substring(0, extension_pos), mediaPath);

                    if (!texpath.Equals(""))
                    {
                        //solo hay un mapa de bits
                        textures.Add(TextureLoader.FromFile(GuiController.Instance.D3dDevice, texpath));
                    }
                    
                }
            }
            
        }
	};

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
        private string type;
        private float angle;
        private float scaleX;
        private float scaleY;
        private float sSpeed;
        private float tSpeed;
        private QWaveForm waveForm;
        private QWaveForm turbulance;

        #region Accesors
		public string Type
        {
            get { return type; }
            set { type = value; }
        }

        public float Angle
        {
            get { return angle; }
            set { angle = value; }
        }

        public float ScaleX
        {
            get { return scaleX; }
            set { scaleX = value; }
        }

        public float ScaleY
        {
            get { return scaleY; }
            set { scaleY = value; }
        }

        public float SSpeed
        {
            get { return sSpeed; }
            set { sSpeed = value; }
        }

        public float TSpeed
        {
            get { return tSpeed; }
            set { tSpeed = value; }
        }

        public QWaveForm WaveForm
        {
            get { return waveForm; }
            set { waveForm = value; }
        }

        public QWaveForm Turbulance
        {
            get { return turbulance; }
            set { turbulance = value; }
        } 

	#endregion
    }

    public class QWaveForm
    {
        private string name;
        private float bas;
        private float amp;
        private float phase;
        private float freq;

        #region Accesors
		public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public float Bas
        {
            get { return bas; }
            set { bas = value; }
        }

        public float Amp
        {
            get { return amp; }
            set { amp = value; }
        }

        public float Phase
        {
            get { return phase; }
            set { phase = value; }
        }

        public float Freq
        {
            get { return freq; }
            set { freq = value; }
        } 
	#endregion
    }

    public class StageCode
    {
        private List<string> _vertexLines = new List<string>();
        private List<string> _pixelLines = new List<string>();

        public List<string> VertexLines
        {
            get { return _vertexLines; }
            set { _vertexLines = value; }
        }

        public List<string> PixelLines
        {
            get { return _pixelLines; }
            set { _pixelLines = value; }
        }
    }

    /// <summary>
    /// Estuctura de Shader
    /// </summary>
    public class ShaderCode
    {
        private List<string> _globals = new List<string>();
        private List<string> VSStruct = new List<string>();
        private List<string> PSStruct = new List<string>();
        private List<string> _auxFuntions = new List<string>();
        private List<StageCode> stages = new List<StageCode>();

        #region Accesors
        public List<string> Globals
        {
            get { return _globals; }
            set { _globals = value; }
        }

        public List<string> VsStruct
        {
            get { return VSStruct; }
            set { VSStruct = value; }
        }

        public List<string> PsStruct
        {
            get { return PSStruct; }
            set { PSStruct = value; }
        }

        public List<string> AuxFuntions
        {
            get { return _auxFuntions; }
            set { _auxFuntions = value; }
        }

        public List<StageCode> Stages
        {
            get { return stages; }
            set { stages = value; }
        } 
        #endregion

        public string Build()
        {
            //genero el texto Propiamente diacho
            string ShaderText = "//prueba de generacion automatica de shaders\n";

            //agrego las variables globales
            foreach (string s in Globals)
            {
                ShaderText += s + "\n";
            }

            //Espacios           
            ShaderText += "\n";
            ShaderText += "\n";

            //Estructura VSInput
            ShaderText += "struct VSInput\n";
            ShaderText += "{\n";
            foreach (string s in VsStruct)
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
            foreach (string s in PsStruct)
            {
                ShaderText += "\t" + s + "\n";
            }
            ShaderText += "};\n";

            //Espacios           
            ShaderText += "\n";
            ShaderText += "\n";

            //funciones auxiliares
            foreach (string s in AuxFuntions)
            {
                ShaderText += s + "\n";
            }

            for (int i = 0; i < Stages.Count; i++)
            {
                StageCode stage = Stages[i];

                //Espacios           
                ShaderText += "\n";
                ShaderText += "\n";

                //Funcion del vertexShader
                ShaderText += "PSInput VertexShader" + i + "(VSInput In)\n";
                ShaderText += "{\n";
                foreach (string s in stage.VertexLines)
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
                foreach (string s in stage.PixelLines)
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
            for (int i = 0; i < Stages.Count; i++)
            {
                StageCode stage = Stages[i];

                if (i > 0)
                {
                    //Espacios           
                    ShaderText += "\n";
                    ShaderText += "\n";
                }

                //genero la pasada
                ShaderText += "\tpass p" + i + "\n";
                ShaderText += "\t{\n";
                ShaderText += "\t\tVertexShader = compile vs_3_0 VertexShader" + i + "();\n";
                ShaderText += "\t\tPixelShader = compile ps_3_0 PixelShader" + i + "();\n";
                ShaderText += "\t}\n";
            }
            ShaderText += "}\n";
            

            return ShaderText;
        }
        
        public StageCode NewStage()
        {
            StageCode stage = new StageCode();
            Stages.Add(stage);
            return stage;
        }
    }

    /// <summary>
    /// Parser de Shaders
    /// </summary>
    public static class Q3ShaderParser
    {
        /// <summary>
        /// Cargar shader desde un archivo
        /// </summary>
        public static List<QShaderData> GetFxFromFile(string file, string mediaPath)
        {
            return GetFxFromText(File.ReadAllText(file), mediaPath);
        }

        /// <summary>
        /// Cargar shader
        /// </summary>
        public static List<QShaderData> GetFxFromText(string code, string mediaPath)
        {
            List<QShaderData> shaders = new List<QShaderData>();
            QShaderTokenizer tokenizer = new QShaderTokenizer(code);

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
                        shader.BuildFx();
                        foreach (QShaderStage qStage in shader.Stages)
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
        /// Parsear shader
        /// </summary>
        private static QShaderData ParseShader(QShaderTokenizer tokenizer)
        {
            QShaderData qsd = new QShaderData();

            qsd.Name = tokenizer.GetNext();

            string token = tokenizer.GetNext();

            if (!token.Equals("{"))
            {
                return null;
            }

            // Parse a shader
	        while(!tokenizer.EOF) {
		        token = tokenizer.GetNext().ToLower();

		        if(token.Equals("}")) { break; }
        		
		        switch (token) {
			        case "{":
				        QShaderStage stage = ParseStage(tokenizer);
        				
				        // I really really really don't like doing this, which basically just forces lightmaps to use the 'filter' blendmode
				        // but if I don't a lot of textures end up looking too bright. I'm sure I'm jsut missing something, and this shouldn't
				        // be needed.
				        if(stage.IsLightMap() && (stage.HasBlendFunc))
                        {
					        stage.BlendSrc = "GL_DST_COLOR";
					        stage.BlendDest = "GL_ZERO";
				        }
        				
				        // I'm having a ton of trouble getting lightingSpecular to work properly, 
				        // so this little hack gets it looking right till I can figure out the problem
				        if(stage.AlphaGen.Equals("lightingspecular"))
                        {
					        stage.BlendSrc = "GL_ONE";
					        stage.BlendDest = "GL_ZERO";
					        stage.HasBlendFunc = false;
					        stage.DepthWrite = true;
				        }
        				
				        if(stage.HasBlendFunc)
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
		                QShaderDeform deform = new QShaderDeform() {Type = tokenizer.GetNext().ToLower()};
        				
				        switch(deform.Type) {
					        case "wave":
						        deform.Spread = 1.0f / ParserTools.ToFloat(tokenizer.GetNext());
						        deform.WaveForm = ParseWaveform(tokenizer);
						        break;
					        default: deform = null; break;
				        }
        				
				        if(deform != null) { qsd.VertexDeforms.Add(deform); }
				        break;
        				
			        case "sort":
				        var sort = tokenizer.GetNext().ToLower();
				        switch(sort) {
					        case "portal": qsd.Sort = 1; break;
					        case "sky": qsd.Sort = 2; break;
					        case "opaque": qsd.Sort = 3; break;
					        case "banner": qsd.Sort = 6; break;
					        case "underwater": qsd.Sort = 8; break;
					        case "additive": qsd.Sort = 9; break;
					        case "nearest": qsd.Sort = 16; break;
					        default: qsd.Sort = int.Parse(sort); break; 
				        };
				        break;
        				
			        case "surfaceparm":
				        var param = tokenizer.GetNext().ToLower();
        				
				        switch(param) {
					        case "sky":
						        qsd.Sky = true;
						        break;
					        default: break;
				        }
				        break;
        				
			        default: break;
		        }
	        }

            if (qsd.Sort > 0)
            {
                qsd.Sort = (qsd.Opaque ? 3 : 9);
            }

            return qsd;
	
        }

        private static QShaderStage ParseStage(QShaderTokenizer tokenizer)
        {
            QShaderStage stage = new QShaderStage();
            // Parse a Stage
            while (!tokenizer.EOF)
            {
                string token = tokenizer.GetNext();
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
                        Match match = Regex.Match(nextMap, @"(\.jpg|\.tga)");
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
                        var tcMod = new QTcMod()
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
                                tcMod.Turbulance = new QWaveForm()
                                                       {
                                                           Name = char.IsLetter(tokenizer.GetToken()[0])
                                                                      ? tokenizer.GetNext() : "",
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
            return new QWaveForm()
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
    /// Convertidor experimental de Shaders de Quake 3 a Shaders de DirectX.
    /// No abarca todas las posibilidades.
    /// </summary>
    public static class QShaderBuilderDX
    {
        
        public static string BuildShaderSource(QShaderData shader)
        {
            //devuelve el codigo del Fx
            ShaderCode shaderCode = new ShaderCode();


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



            foreach (QShaderStage qstage in shader.Stages)
            {
                StageCode stageCode = shaderCode.NewStage();
                stageCode.VertexLines = BuildVertexShader(shader, qstage);
                stageCode.PixelLines = BuildPixelShader(shader, qstage);
            }

            return shaderCode.Build();
        }

        private static List<string> BuildPixelShader(QShaderData shader, QShaderStage stage)
        {
            List<string> PixelLines = new List<string>();
            
	        PixelLines.Add("float4 texColor = tex2D(texture0, In.Tex0);");
        	
	        switch(stage.RgbGen)
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

            switch(stage.AlphaGen)
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


            if(stage.AlphaFunc != null) {
		        switch(stage.AlphaFunc)
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
            List<string> VertexLines = new List<string>();
            
            VertexLines.Add("PSInput Out = In;");
	        VertexLines.Add("float4 defPosition = In.Pos;");
        	
	        for(int i = 0; i < shader.VertexDeforms.Count; ++i) {

		        QShaderDeform deform = shader.VertexDeforms[i];
        		
		        switch(deform.Type)
		        {
		            case "wave":
		                string name = "deform" + i;
		                string offName = "deformOff" + i;

		                VertexLines.Add(
		                    "float " + offName + " = (In.Pos.x + In.Pos.y + In.Pos.z) * " +
		                    ParserTools.ToString(deform.Spread) + ";");

		                float phase = deform.WaveForm.Phase;
		                //deform.WaveForm.Phase = phase.toFixed(4) + ' + ' + offName; <-----MIRAR ESTA LINEA
		                VertexLines.Add(CreateWaveForm(name, deform.WaveForm, "g_time"));
		                deform.WaveForm.Phase = phase;

		                VertexLines.Add("defPosition += float4(In.Normal * " + name + ",0);");
		                break;
		            default:
		                break;
		        }
	        }
        	
	        VertexLines.Add("float4 worldPosition = mul( defPosition, g_mWorld );");
            VertexLines.Add("Out.Color = In.Color;");
	        
	        if(stage.TcGen.Equals("environment"))
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
	        for(int i = 0; i < stage.TcMods.Count; ++i)
            {
		        QTcMod tcMod = stage.TcMods[i];

		        switch(tcMod.Type) {
			        case "rotate":
		                VertexLines.Add("float r = " + ParserTools.ToString(tcMod.Angle) + " * g_time;");
                        VertexLines.Add("Out.Tex0 -= float2(0.5, 0.5);");
                        VertexLines.Add("Out.Tex0 = float2(Out.Tex0.x * cos(r) - Out.Tex0.y * sin(r), Out.Tex0.y * cos(r) + Out.Tex0.x * sin(r));");
                        VertexLines.Add("Out.Tex0 += float2(0.5, 0.5);");
				        break;

			        case "scroll":
		                VertexLines.Add(
		                    "Out.Tex0 += float2(" + ParserTools.ToString(tcMod.SSpeed) + " * g_time, " + ParserTools.ToString(tcMod.TSpeed) +
		                    " * g_time);");
		                break;

			        case "scale":
		                VertexLines.Add(
		                    "Out.Tex0 *= float2(" + ParserTools.ToString(tcMod.ScaleX) + ", " + ParserTools.ToString(tcMod.ScaleY) + ");"
		                    );
		                break;

			        case "stretch":
				        VertexLines.Add( CreateWaveForm("stretchWave", tcMod.WaveForm,""));
				        VertexLines.Add("stretchWave = 1.0 / stretchWave;");
                        VertexLines.Add("Out.Tex0 *= stretchWave;");
		                VertexLines.Add("Out.Tex0 += float2(0.5 - (0.5 * stretchWave), 0.5 - (0.5 * stretchWave));");
		                break;

			        case "turb":
				        var tName = "turbTime" + i;
		                VertexLines.Add("float " + tName + " = " + ParserTools.ToString(tcMod.Turbulance.Phase) + " + g_time * " +
		                                ParserTools.ToString(tcMod.Turbulance.Freq) + ";");
		                VertexLines.Add("Out.Tex0.x += sin( ( ( In.Pos.x + In.Pos.z )* 1.0/128.0 * 0.125 + " + tName +
		                                " ) * 6.283) * " + ParserTools.ToString(tcMod.Turbulance.Amp) + ";");
		                VertexLines.Add("Out.Tex0.y += sin( ( In.Pos.y * 1.0/128.0 * 0.125 + " + tName + " ) * 6.283) * " +
		                                ParserTools.ToString(tcMod.Turbulance.Amp) + ";");
				        break;

			        default:
		                break;
		        }
	        }
        	
	        switch(stage.AlphaGen) {
		        case "lightingspecular":
			        VertexLines.Add("Out.Tex1 = In.Tex1;" );
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
            if(wf==null)
            {
                return "float " + name + " = 0.0;";
            }

            if(timeVar.Equals(""))
            {
                timeVar = "g_time";
            }

            string funcName = "";

            switch(wf.Name)
            {

                case "sin":
                    //retorna: float name = base + sin(( pha + t*freq )*2PI)*Amp;
                    return "float " + name + " = " + ParserTools.ToString(wf.Bas) + " + sin((" + ParserTools.ToString(wf.Phase) + " + " +
                           timeVar + " * " + ParserTools.ToString(wf.Freq) + ") * 6.283) * " + ParserTools.ToString(wf.Amp) + ";";

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
            return "float " + name + " = " + ParserTools.ToString(wf.Bas) + " + " + funcName + "(" + ParserTools.ToString(wf.Phase) + " + " +
                   timeVar + " * " + ParserTools.ToString(wf.Freq) + ") * " + ParserTools.ToString(wf.Amp) + ";";
        
            
        }
    }
}
