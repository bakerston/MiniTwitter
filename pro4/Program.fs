open System
open Akka.FSharp
open System.IO
open System.Security.Cryptography
open Akka.Configuration

type SearchMessage = {
    finalDestination: int
    currDestination: int
    numHops: int
}

type RequestMessage = {
    requestList: list<int>  
}

type FingerTableMessage = {
    total_count: int
    fingerList: list<int>
}

let mutable total_counts = 0
let mutable total_hops = 0
let system = System.create "Project3" (Configuration.load())

let createLineActors (num: int) =
    spawn system ("Actor" + num.ToString())
        (fun mailbox ->          
            let mutable cur_num = num |> int
            let mutable fingerList = []
            let mutable newFinger = []
            let mutable fingerLength = 0
            let mutable total_count = 0
            let buildTime = Diagnostics.Stopwatch()
            buildTime.Start()
            let rec loop() = actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()             
                match box message with
                | :? int as msg ->
                    printfn "%i" msg
                | :? string as msg ->
                    printfn "%s" msg
                | :? RequestMessage as msg ->
                    let mutable n = List.length msg.requestList
                    
                    for i = 1 to n do
                        let mutable index = 0
                        let mutable next_dest = newFinger.[index]
                        //printfn "%i reveived search message" num
                        let mutable final_dest = msg.requestList.[i - 1]
                        if final_dest < num then
                            final_dest <- final_dest + (int)(( ** ) 2. ((float)fingerLength))
                        while index < fingerLength && final_dest >= next_dest do
                            if next_dest = final_dest then
                                total_counts <- total_counts + 1
                                total_hops <- total_hops + 1
                                index <- fingerLength
                                printfn "count = %i" total_counts
                                if total_counts = total_count then
                                    printfn "======================="
                                    printfn "count = %i" total_counts
                                    printfn "hops = %i" total_hops
                                    printfn "======================="

                            else if index = fingerLength - 1 || newFinger.[index + 1] > final_dest then
                                let mutable currDestination = fingerList.[index]
                                //printfn "%i sent message to %i" num destination
                                if final_dest >= (int)(( ** ) 2. ((float)fingerLength)) then
                                    final_dest <- final_dest - (int)(( ** ) 2. ((float)fingerLength))
                                let newMessage = {currDestination = currDestination; finalDestination = final_dest; numHops = 1}
                                let destActor = system.ActorSelection("akka://Project3/user/Actor" + currDestination.ToString())
                                destActor <! newMessage
                                index <- fingerLength
                            else
                                index <- index + 1
                                next_dest <- newFinger.[index]

                | :? SearchMessage as msg ->                  
                    let mutable index = 0
                    let mutable next_dest = newFinger.[index]
                    //printfn "%i reveived search message" num
                    let mutable final_dest = msg.finalDestination
                    if final_dest < num then
                        final_dest <- final_dest + (int)(( ** ) 2. ((float)fingerLength))
                    while index < fingerLength && final_dest >= next_dest do
                        if next_dest = final_dest then                            
                            total_counts <- total_counts + 1
                            total_hops <- total_hops + msg.numHops
                            index <- fingerLength
                            printfn "count = %i" total_counts
                            if total_counts = total_count then
                                printfn "======================="
                                printfn "count = %i" total_counts
                                printfn "hops = %i" total_hops             
                                printfn "======================="

                        else if index = fingerLength - 1 || newFinger.[index + 1] > final_dest then
                            let mutable currDestination = fingerList.[index]
                            //printfn "%i sent message to %i" num destination
                            let newMessage = {currDestination = currDestination; finalDestination = msg.finalDestination; numHops = msg.numHops + 1}
                            let destActor = system.ActorSelection("akka://Project3/user/Actor" + currDestination.ToString())
                            destActor <! newMessage
                            index <- fingerLength
                        else
                            index <- index + 1
                            next_dest <- newFinger.[index]
                    Threading.Thread.Sleep(1000)
                | :? FingerTableMessage as msg ->
                    fingerList <- msg.fingerList
                    fingerLength <- List.length fingerList
                    total_count <- msg.total_count
                    let increment = (int)(( ** ) 2. ((float)fingerLength))
                    for i = 0 to fingerLength - 1 do
                        if fingerList.[i] < num then
                            newFinger <- [fingerList.[i] + increment] |> List.append newFinger
                        else 
                            newFinger <- [fingerList.[i]] |> List.append newFinger
                    //printfn "node %i 's newfinger = %A" num newFinger
                | _ -> () 
                return! loop()
            }
            loop()
        )

[<EntryPoint>]
let main argv =
    printf "input the number of nodes: "
    let numNode = Console.ReadLine() |> int
    printf "input the number of requests: "
    let numReq = Console.ReadLine() |> int

    let expected_keys = numNode * numNode

    // m: number of digits of the 01 hash code
    let m = (1. + Math.Log2((float)expected_keys)) |> int
    // Total possible key on the circle.
    let total_keys = (int)(( ** ) 2. ((float)m))
    // Total pass counts for all requests from every nodes 
    //printfn "m = %i"  m
    //printfn "total = %i" total_keys
    let rec binSearch target arr =
        match Array.length arr with
          | 0 -> None
          | i -> let middle = i / 2
                 match  sign <| compare target arr.[middle] with
                   | 0  -> Some(target)
                   | -1 -> binSearch target arr.[..middle-1]
                   | _  -> binSearch target arr.[middle+1..]
    let tt = binSearch 7 [|3..10|] 
    //printf "array = %A" [|3..10|] 
    //printf "tt=%A\n" tt
    let sha1(s:String) = 
        let b = Text.Encoding.ASCII.GetBytes(s)
        let bsha1 = Security.Cryptography.SHA1.Create().ComputeHash(b)
        let hex = Array.map (fun (x : byte) -> System.String.Format("{0:X2}", x)) bsha1
        let result = String.concat System.String.Empty hex
        result

    let fromHex (s:string) = 
      s
      |> Seq.windowed 2
      |> Seq.mapi (fun i j -> (i,j))
      |> Seq.filter (fun (i,j) -> i % 2=0)
      |> Seq.map (fun (_,j) -> Byte.Parse(new System.String(j),System.Globalization.NumberStyles.AllowHexSpecifier))
      |> Array.ofSeq
    let str = char(100).ToString()
    let res = sha1(str)
    let res2 = fromHex(res)

    // Initalize position of each node
    let actor_pos = [
        for i = 1 to numNode do
            let mutable pos = Random().Next((i - 1)*(int)(total_keys/numNode), i*(int)(total_keys/numNode))
            pos
        ]
    printfn "actor_position = %A" actor_pos

    let actor_list = [
        for i = 0 to numNode - 1 do
            createLineActors(actor_pos.[i])
    ]

    //build finger table for each node
    let next_list = [for i in actor_pos -> i + total_keys]
    let double_list = actor_pos @ next_list
    //printfn "%A" next_list
    //printfn "%A" double_list
    let mutable cur_pos = 0
    let mutable nxt_pos = 0
    let mutable suc_idx = 0
    let mutable final_idx = 0
    //printfn "m = %A"  m

    let mutable cur_list: list<int> = []
    for i = 0 to numNode - 1 do
        suc_idx <- i + 1
        suc_idx <- suc_idx % numNode
        cur_pos <- double_list.[i]
        nxt_pos <- 0
        cur_list <- []
        for step = 0 to m - 1 do    
            nxt_pos <- cur_pos + (int)(( ** ) 2. ((float)step))
            while double_list.[suc_idx] < nxt_pos do
                suc_idx <- suc_idx + 1
            final_idx <- suc_idx % numNode
            cur_list <- [double_list.[final_idx]] |> List.append cur_list
        //printfn "%A"  i
        //printfn "%A" cur_list
        let message = {fingerList = cur_list; total_count = numNode * numReq}
        actor_list.Item(i) <! message

    
    let mutable req = 0
    for i = 0 to numNode - 1 do  
        let mutable req_list: list<int> = []
        for j = 1 to numReq do
            req <- Random().Next(0, numNode)
            while req = i do
                req <- Random().Next(0, numNode)
            req_list <- [actor_pos.[req]] |> List.append req_list
        //printfn "node index = %i" i
        //printfn "req_list = %A" req_list
        let message = {requestList = req_list}
        actor_list.Item(i) <! message

    //printfn "total counts = %i" total_counts

    //let System_wait=0
    while total_counts < (numNode * numReq)/2 do
        //System_wait |> ignore
        printfn "global_counts is %i nodes" total_counts
        printfn "global_hops is %i nodes" total_hops
        Threading.Thread.Sleep(1000)   
    printfn "////////////////////////////"
    let ans = ((float)total_hops)/((float)total_counts)
    printfn "%f" ans
    
        
    Console.ReadLine() |> ignore
    0 // return an integer exit code