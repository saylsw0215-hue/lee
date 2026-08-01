using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HeroDefense.Editor
{
    /// <summary>Extracts transparent runtime hero sprites from the generated 3x2 character sheet.</summary>
    public static class HeroCharacterArtSetup
    {
        private const string SourcePath="Assets/Art/Characters/Heroes/Source/HeroesChibiSheet-Chroma.png";
        private const string OutputDirectory="Assets/Resources/HeroArt";
        private readonly struct Entry
        {
            public readonly string Id; public readonly int Column; public readonly int Row;
            public Entry(string id,int column,int row){Id=id;Column=column;Row=row;}
        }
        private static readonly Entry[] Entries={
            new("hero_arden_knight",0,1),new("hero_rian_ranger",1,1),new("hero_sera_fire_mage",2,1),
            new("hero_elia_saint",0,0),new("hero_nox_assassin",1,0),new("hero_kai_engineer",2,0)};

        [InitializeOnLoadMethod]
        private static void ScheduleFirstImport()
        {
            EditorApplication.delayCall+=()=>
            {
                string sample=$"{OutputDirectory}/{Entries[0].Id}_full.png";
                if(File.Exists(SourcePath)&&(AssetImporter.GetAtPath(sample) as TextureImporter)?.textureType!=TextureImporterType.Sprite)Setup();
            };
        }

        [MenuItem("Tools/Hero Defense/Art/Setup Hero Character Images")]
        public static void Setup()
        {
            TextureImporter sourceImporter=AssetImporter.GetAtPath(SourcePath) as TextureImporter;
            if(sourceImporter==null)throw new FileNotFoundException("Hero character sheet was not found.",SourcePath);
            sourceImporter.isReadable=true;sourceImporter.textureCompression=TextureImporterCompression.Uncompressed;sourceImporter.SaveAndReimport();
            Texture2D source=AssetDatabase.LoadAssetAtPath<Texture2D>(SourcePath);
            if(source==null||source.width!=1536||source.height!=1024)throw new InvalidOperationException("Hero sheet must be 1536x1024 with a 3x2 grid.");
            Directory.CreateDirectory(OutputDirectory);
            foreach(Entry entry in Entries)
            {
                Color[] cell=source.GetPixels(entry.Column*512,entry.Row*512,512,512);
                RemoveChroma(cell);
                WritePng($"{OutputDirectory}/{entry.Id}_full.png",cell,512,512);
                Color[] portrait=Crop(cell,512,96,176,320,320);
                WritePng($"{OutputDirectory}/{entry.Id}_portrait.png",portrait,320,320);
            }
            AssetDatabase.Refresh();
            foreach(Entry entry in Entries){ConfigureSprite($"{OutputDirectory}/{entry.Id}_full.png",512);ConfigureSprite($"{OutputDirectory}/{entry.Id}_portrait.png",320);}
            sourceImporter.isReadable=false;sourceImporter.textureCompression=TextureImporterCompression.Compressed;sourceImporter.SaveAndReimport();
            AssetDatabase.SaveAssets();Debug.Log($"Hero character images prepared in {OutputDirectory}.");
        }

        private static void RemoveChroma(Color[] pixels)
        {
            for(int i=0;i<pixels.Length;i++)
            {
                Color c=pixels[i];
                if(c.g>.68f&&c.g>c.r*1.22f&&c.g>c.b*1.22f)
                {
                    float distance=Mathf.Sqrt(c.r*c.r+(1-c.g)*(1-c.g)+c.b*c.b);
                    c.a*=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.07f,.3f,distance));
                    c.g=Mathf.Min(c.g,Mathf.Max(c.r,c.b)*1.35f);
                }
                pixels[i]=c;
            }
        }
        private static Color[] Crop(Color[] source,int sourceWidth,int x,int y,int width,int height)
        {
            Color[] result=new Color[width*height];
            for(int row=0;row<height;row++)Array.Copy(source,(y+row)*sourceWidth+x,result,row*width,width);
            return result;
        }
        private static void WritePng(string path,Color[] pixels,int width,int height)
        {
            var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);texture.SetPixels(pixels);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
        }
        private static void ConfigureSprite(string path,int maxSize)
        {
            if(AssetImporter.GetAtPath(path) is not TextureImporter importer)return;
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.isReadable=false;importer.filterMode=FilterMode.Bilinear;importer.textureCompression=TextureImporterCompression.Compressed;importer.maxTextureSize=maxSize;importer.SaveAndReimport();
        }
    }
}
