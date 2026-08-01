import Foundation
import CoreGraphics
import ImageIO
import UniformTypeIdentifiers

let root=URL(fileURLWithPath:FileManager.default.currentDirectoryPath)
let colorSpace=CGColorSpaceCreateDeviceRGB()
func load(_ path:String)->(Int,Int,[UInt8]) {
    let url=root.appendingPathComponent(path)
    guard let source=CGImageSourceCreateWithURL(url as CFURL,nil),let image=CGImageSourceCreateImageAtIndex(source,0,nil) else{fatalError("Cannot load \(path)")}
    var bytes=[UInt8](repeating:0,count:image.width*image.height*4)
    bytes.withUnsafeMutableBytes{raw in let context=CGContext(data:raw.baseAddress,width:image.width,height:image.height,bitsPerComponent:8,bytesPerRow:image.width*4,space:colorSpace,bitmapInfo:CGImageAlphaInfo.premultipliedLast.rawValue)!;context.draw(image,in:CGRect(x:0,y:0,width:image.width,height:image.height))}
    return(image.width,image.height,bytes)
}
func crop(_ source:[UInt8],_ sourceWidth:Int,_ x:Int,_ y:Int,_ width:Int,_ height:Int)->[UInt8] {
    var result=[UInt8](repeating:0,count:width*height*4)
    for row in 0..<height{result.replaceSubrange(row*width*4..<(row+1)*width*4,with:source[((y+row)*sourceWidth+x)*4..<((y+row)*sourceWidth+x+width)*4])}
    return result
}
func save(_ path:String,_ pixels:[UInt8],_ width:Int,_ height:Int) {
    var data=pixels
    let image:CGImage=data.withUnsafeMutableBytes{raw in let context=CGContext(data:raw.baseAddress,width:width,height:height,bitsPerComponent:8,bytesPerRow:width*4,space:colorSpace,bitmapInfo:CGImageAlphaInfo.premultipliedLast.rawValue)!;return context.makeImage()!}
    let url=root.appendingPathComponent(path);try! FileManager.default.createDirectory(at:url.deletingLastPathComponent(),withIntermediateDirectories:true)
    let destination=CGImageDestinationCreateWithURL(url as CFURL,UTType.png.identifier as CFString,1,nil)!;CGImageDestinationAddImage(destination,image,nil);CGImageDestinationFinalize(destination)
}

let (worldWidth,_,world)=load("Assets/Art/World/Source/MedievalWorldConcept.png")
save("Assets/Resources/Backgrounds/MainKingdom.png",crop(world,worldWidth,0,0,1402,505),1402,505)
let stages=[("Grassland",8,515,326,237),("DeepForest",344,515,340,237),("GoldenDesert",694,515,337,237),("DarkFortress",1042,515,348,237)]
for(name,x,y,w,h)in stages{save("Assets/Resources/Backgrounds/Stage\(name).png",crop(world,worldWidth,x,y,w,h),w,h)}

let (sheetWidth,_,sheet)=load("Assets/Art/World/Source/ProductionBuildings-Chroma.png")
let ids=[("building_barracks",0,0),("building_archery_range",1,0),("building_magic_tower",2,0),("building_guard_barracks",0,1),("building_siege_workshop",1,1),("building_sanctuary",2,1)]
for(id,column,row)in ids{
    var cell=crop(sheet,sheetWidth,column*512,row*512,512,512)
    for i in stride(from:0,to:cell.count,by:4){let r=Float(cell[i])/255,g=Float(cell[i+1])/255,b=Float(cell[i+2])/255;if r > 0.68 && b > 0.68 && r > g*1.2 && b > g*1.2{let d=sqrt((1-r)*(1-r)+g*g+(1-b)*(1-b));let t=max(0,min(1,(d-0.07)/0.24));let smooth=t*t*(3-2*t);cell[i+3]=UInt8(Float(cell[i+3])*smooth);let limit=g*1.35;cell[i]=UInt8(min(r,limit)*255);cell[i+2]=UInt8(min(b,limit)*255)}}
    save("Assets/Resources/BuildingArt/\(id).png",cell,512,512)
}
print("Prepared menu, four stage backgrounds, and six building sprites.")
