import Foundation
import CoreGraphics
import ImageIO
import UniformTypeIdentifiers
let root=URL(fileURLWithPath:FileManager.default.currentDirectoryPath),space=CGColorSpaceCreateDeviceRGB()
func load(_ p:String)->(Int,Int,[UInt8]){let u=root.appendingPathComponent(p),s=CGImageSourceCreateWithURL(u as CFURL,nil)!,i=CGImageSourceCreateImageAtIndex(s,0,nil)!;var b=[UInt8](repeating:0,count:i.width*i.height*4);b.withUnsafeMutableBytes{r in let c=CGContext(data:r.baseAddress,width:i.width,height:i.height,bitsPerComponent:8,bytesPerRow:i.width*4,space:space,bitmapInfo:CGImageAlphaInfo.premultipliedLast.rawValue)!;c.draw(i,in:CGRect(x:0,y:0,width:i.width,height:i.height))};return(i.width,i.height,b)}
func crop(_ s:[UInt8],_ sw:Int,_ x:Int,_ y:Int,_ w:Int,_ h:Int)->[UInt8]{var o=[UInt8](repeating:0,count:w*h*4);for r in 0..<h{o.replaceSubrange(r*w*4..<(r+1)*w*4,with:s[((y+r)*sw+x)*4..<((y+r)*sw+x+w)*4])};return o}
func save(_ p:String,_ d0:[UInt8],_ w:Int,_ h:Int){var d=d0;let i:CGImage=d.withUnsafeMutableBytes{r in CGContext(data:r.baseAddress,width:w,height:h,bitsPerComponent:8,bytesPerRow:w*4,space:space,bitmapInfo:CGImageAlphaInfo.premultipliedLast.rawValue)!.makeImage()!};let u=root.appendingPathComponent(p);try! FileManager.default.createDirectory(at:u.deletingLastPathComponent(),withIntermediateDirectories:true);let o=CGImageDestinationCreateWithURL(u as CFURL,UTType.png.identifier as CFString,1,nil)!;CGImageDestinationAddImage(o,i,nil);CGImageDestinationFinalize(o)}
func key(_ input:[UInt8])->[UInt8]{var p=input;for i in stride(from:0,to:p.count,by:4){let r=Float(p[i])/255,g=Float(p[i+1])/255,b=Float(p[i+2])/255;if r > 0.68 && b > 0.68 && r > g*1.2 && b > g*1.2{let d=sqrt((1-r)*(1-r)+g*g+(1-b)*(1-b)),t=max(0,min(1,(d-0.07)/0.24)),a=t*t*(3-2*t);p[i+3]=UInt8(Float(p[i+3])*a);let l=g*1.35;p[i]=UInt8(min(r,l)*255);p[i+2]=UInt8(min(b,l)*255)}};return p}
let ids=["hero_arden_knight","hero_rian_ranger","hero_sera_fire_mage","hero_kai_engineer","hero_elia_saint"]
let(iw,ih,ingame)=load("Assets/Art/Characters/TeranSource/TeranHeroes-Chroma.png"),cell=iw/5
let margins=[8,12,18,32,8]
for i in 0..<5{let margin=margins[i],width=cell-margin*2,image=key(crop(ingame,iw,i*cell+margin,0,width,ih));save("Assets/Resources/HeroArt/\(ids[i])_full.png",image,width,ih);save("Assets/Resources/HeroArt/\(ids[i])_portrait.png",crop(image,width,max(0,(width-300)/2),110,min(300,width),300),min(300,width),300)}
let(sw,_,sheet)=load("Assets/Art/Characters/TeranSource/TeranHeroConcept.png")
for i in 0..<5{
    let cardX=16+i*300;save("Assets/Resources/HeroPosters/\(ids[i]).png",crop(sheet,sw,cardX,104,300,600),300,600)
    save("Assets/Resources/SkillArt/\(ids[i])_active.png",crop(sheet,sw,cardX+8,596,66,66),66,66)
    save("Assets/Resources/SkillArt/\(ids[i])_ultimate.png",crop(sheet,sw,cardX+224,596,66,66),66,66)
    save("Assets/Resources/HeroEffects/\(ids[i]).png",crop(sheet,sw,736+i*150,752,148,130),148,130)
    save("Assets/Resources/HeroEmotes/\(ids[i])_happy.png",crop(sheet,sw,736+i*150,908,72,72),72,72)
    save("Assets/Resources/HeroEmotes/\(ids[i])_hurt.png",crop(sheet,sw,811+i*150,908,72,72),72,72)
}
print("Prepared five hero sprites, posters, active/ultimate icons, effects, and emote pairs.")
