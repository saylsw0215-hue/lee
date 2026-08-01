import Foundation
import CoreGraphics
import ImageIO
import UniformTypeIdentifiers

let root = URL(fileURLWithPath: FileManager.default.currentDirectoryPath)
let input = root.appendingPathComponent("Assets/Art/Characters/Heroes/Source/HeroesChibiSheet-Chroma.png")
let output = root.appendingPathComponent("Assets/Resources/HeroArt")
try FileManager.default.createDirectory(at: output, withIntermediateDirectories: true)
guard let source = CGImageSourceCreateWithURL(input as CFURL, nil), let image = CGImageSourceCreateImageAtIndex(source, 0, nil) else { fatalError("Cannot read hero sheet") }
let width=image.width, height=image.height, bytesPerRow=width*4
var pixels=[UInt8](repeating:0,count:height*bytesPerRow)
let colorSpace=CGColorSpaceCreateDeviceRGB()
pixels.withUnsafeMutableBytes { raw in
    let context=CGContext(data:raw.baseAddress,width:width,height:height,bitsPerComponent:8,bytesPerRow:bytesPerRow,space:colorSpace,bitmapInfo:CGImageAlphaInfo.premultipliedLast.rawValue)!
    context.draw(image,in:CGRect(x:0,y:0,width:width,height:height))
}
for i in stride(from:0,to:pixels.count,by:4) {
    let r=Float(pixels[i])/255, g=Float(pixels[i+1])/255, b=Float(pixels[i+2])/255
    if g > 0.68 && g > r*1.22 && g > b*1.22 {
        let d=sqrt(r*r+(1-g)*(1-g)+b*b)
        let t=max(0,min(1,(d-0.07)/0.23)); let smooth=t*t*(3-2*t)
        pixels[i+3]=UInt8(Float(pixels[i+3])*smooth)
        pixels[i+1]=UInt8(min(g,max(r,b)*1.35)*255)
    }
}
func save(_ name:String,_ data:[UInt8],_ w:Int,_ h:Int) {
    var mutable=data
    let cg:CGImage=mutable.withUnsafeMutableBytes { raw in
        let context=CGContext(data:raw.baseAddress,width:w,height:h,bitsPerComponent:8,bytesPerRow:w*4,space:colorSpace,bitmapInfo:CGImageAlphaInfo.premultipliedLast.rawValue)!
        return context.makeImage()!
    }
    let url=output.appendingPathComponent(name)
    let destination=CGImageDestinationCreateWithURL(url as CFURL,UTType.png.identifier as CFString,1,nil)!
    CGImageDestinationAddImage(destination,cg,nil); CGImageDestinationFinalize(destination)
}
func crop(_ x:Int,_ y:Int,_ w:Int,_ h:Int)->[UInt8] {
    var result=[UInt8](repeating:0,count:w*h*4)
    for row in 0..<h { result.replaceSubrange(row*w*4..<(row+1)*w*4,with:pixels[(y+row)*bytesPerRow+x*4..<(y+row)*bytesPerRow+(x+w)*4]) }
    return result
}
let entries=[("hero_arden_knight",0,0),("hero_rian_ranger",1,0),("hero_sera_fire_mage",2,0),("hero_elia_saint",0,1),("hero_nox_assassin",1,1),("hero_kai_engineer",2,1)]
for (id,column,row) in entries {
    let cell=crop(column*512,row*512,512,512); save("\(id)_full.png",cell,512,512)
    var portrait=[UInt8](repeating:0,count:320*320*4)
    for line in 0..<320 { portrait.replaceSubrange(line*320*4..<(line+1)*320*4,with:cell[(96+(80+line)*512)*4..<(96+(80+line)*512+320)*4]) }
    save("\(id)_portrait.png",portrait,320,320)
}
print("Prepared \(entries.count) hero image pairs in \(output.path)")
