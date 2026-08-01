import Foundation
import CoreGraphics
import ImageIO
import UniformTypeIdentifiers
let root=URL(fileURLWithPath:FileManager.default.currentDirectoryPath),space=CGColorSpaceCreateDeviceRGB()
func load(_ path:String)->(Int,Int,[UInt8]){let u=root.appendingPathComponent(path);let s=CGImageSourceCreateWithURL(u as CFURL,nil)!;let i=CGImageSourceCreateImageAtIndex(s,0,nil)!;var b=[UInt8](repeating:0,count:i.width*i.height*4);b.withUnsafeMutableBytes{r in let c=CGContext(data:r.baseAddress,width:i.width,height:i.height,bitsPerComponent:8,bytesPerRow:i.width*4,space:space,bitmapInfo:CGImageAlphaInfo.premultipliedLast.rawValue)!;c.draw(i,in:CGRect(x:0,y:0,width:i.width,height:i.height))};return(i.width,i.height,b)}
func crop(_ s:[UInt8],_ sw:Int,_ x:Int,_ y:Int,_ w:Int,_ h:Int)->[UInt8]{var o=[UInt8](repeating:0,count:w*h*4);for r in 0..<h{o.replaceSubrange(r*w*4..<(r+1)*w*4,with:s[((y+r)*sw+x)*4..<((y+r)*sw+x+w)*4])};return o}
func save(_ path:String,_ p:[UInt8],_ w:Int,_ h:Int){var d=p;let i:CGImage=d.withUnsafeMutableBytes{r in CGContext(data:r.baseAddress,width:w,height:h,bitsPerComponent:8,bytesPerRow:w*4,space:space,bitmapInfo:CGImageAlphaInfo.premultipliedLast.rawValue)!.makeImage()!};let u=root.appendingPathComponent(path);try! FileManager.default.createDirectory(at:u.deletingLastPathComponent(),withIntermediateDirectories:true);let out=CGImageDestinationCreateWithURL(u as CFURL,UTType.png.identifier as CFString,1,nil)!;CGImageDestinationAddImage(out,i,nil);CGImageDestinationFinalize(out)}
func removeMagenta(_ input:[UInt8])->[UInt8]{var p=input;for i in stride(from:0,to:p.count,by:4){let r=Float(p[i])/255,g=Float(p[i+1])/255,b=Float(p[i+2])/255;if r > 0.68 && b > 0.68 && r > g*1.2 && b > g*1.2{let d=sqrt((1-r)*(1-r)+g*g+(1-b)*(1-b));let t=max(0,min(1,(d-0.07)/0.24)),a=t*t*(3-2*t);p[i+3]=UInt8(Float(p[i+3])*a);let limit=g*1.35;p[i]=UInt8(min(r,limit)*255);p[i+2]=UInt8(min(b,limit)*255)}};return p}

let(mw,mh,monsters)=load("Assets/Art/Characters/Source/Monsters-Chroma.png"),cw=mw/3,ch=mh/2
let monsterIds=["monster_slime","monster_goblin","monster_skeleton","monster_orc","monster_dark_knight","monster_dragon"]
for index in 0..<monsterIds.count{let cell=crop(monsters,mw,(index%3)*cw+12,(index/3)*ch,cw-24,ch);save("Assets/Resources/MonsterArt/\(monsterIds[index]).png",removeMagenta(cell),cw-24,ch)}

let(sw,_,sheet)=load("Assets/Art/Characters/Source/HeroMonsterSkillConcept.png")
let skills=[("skill_knight",1080,596),("skill_ranger",1180,596),("skill_mage",1280,596),("skill_saint",1080,690),("skill_assassin",1180,690),("skill_engineer",1280,690)]
for(name,x,y)in skills{save("Assets/Resources/SkillArt/\(name).png",crop(sheet,sw,x,y,92,92),92,92)}
print("Prepared six monster sprites and six skill icons.")
