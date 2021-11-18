open System
open Akka.FSharp
open System.IO
open System.Security.Cryptography
open Akka.Configuration



type registerMsg = {
    user_id: int
}

type operationMsg = {
    operation_id: int
}

////Local Client and Remote Server
let config =
    Configuration.parse
        @"akka {
            actor {
                provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                debug : {
                    receive : on
                    autoreceive : on
                    lifecycle : on
                    event-stream : on
                    unhandled : on
                }
            }
            remote.helios.tcp {
                hostname = ""localhost""
                port = 9001
            }
        }"
let system = System.create "RemoteClient" config
let RemoteServer = system.ActorSelection("akka.tcp://RemoteClient@localhost:9001/user/RemoteServer")


let CreateUsersActor =
    spawn system ("Actor" + "-tag")
        (fun mailbox ->          
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
                | :? registerMsg as msg ->
                    let user_dig = msg.user_id
                    let user_id = "user" + (user_dig |> string)
                    let msgToServer = "reg,"+user_id+",,,,," 
                    let resp = RemoteServer <! msgToServer
                    printfn "[Register]: User %i with user id of %s" user_dig user_id
                    printfn "[Response from Server]: %s" (resp |> string)
                    Threading.Thread.Sleep(10)
                | _ -> () 
                return! loop()
            }
            loop()
        )

let GenerateMessageActor =
    spawn system ("Actor" + "-tag")
        (fun mailbox ->          
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
                | :? operationMsg as msg ->
                    let operation_id = msg.operation_id
                    let mutable operation_type = ""
                    let mutable msgToServer = ""
                    match operation_id with 
                    | 1 ->
                        operation_type <- "sub"
                        let user1 = Random().Next(0, N)

                    | 2 ->

                    | 3 -> 

                    | 4 ->

                    | 5 ->

                    | 6 ->
                    | _ ->
                        printfn "400 Bad Request!"
                | _ -> () 
                return! loop()
            }
            loop()
        )
 

[<EntryPoint>]
let main argv =
    printf "Input the number of users: "
    let N = Console.ReadLine() |> int

    // Create N users with user_id
    for i = 0 to N - 1 do
        let createUserMsg = {user_id = i}
        let resp = CreateUsersActor <! createUserMsg
        printfn "[Register]: User %i" i 

    // Create Zipf Distribution of subscribers.
    let mutable increment = 0
    let mutable user1_id = ""
    let mutable user2_id = ""
    let mutable msgToServer = ""
    for i = 0 to N - 1 do
        increment <- i + 1
        for j in 0..increment..N - 1 do
            if not (i=j) then
                user1_id <- "user" + (i |> string)
                user2_id <- "user" + (j |> string)
                msgToServer <- "sub," + user1_id + ",,," + user2_id + ",,"
                let resp = RemoteServer <! msgToServer
                printfn "[Subscribe]: User %i to User %i" i j
                printfn "[Response from Server]: %s" (resp |> string)

    // Users posts tweets
    let mutable numPost = 0
    let maxPost = (int)(N/5)
    let mutable numTag = 0
    let mutable curTag = 0
    let maxTag = 3
    let mutable numMen = 0
    let mutable curMen = 0
    let maxMen = 3
    let mutable stringTag = ""
    let mutable stringMen = ""
    let mutable tweetContent = ""

    for i = 0 to N - 1 do
        numPost <- maxPost - (int)(i/10)
        user1_id <- "user" + (i|>string)
        for j = 0 to numPost - 1 do
            stringTag <- ""
            stringMen <- ""
            numTag <- Random().Next(0, maxTag + 1) 
            numMen <- Random().Next(0, maxMen + 1)
            if numTag >= 1 then
                curTag <- Random().Next(0, N)
                stringTag <- "Tag" + (curTag |> string)
                if numTag > 1 then
                    for k = 1 to numTag do
                        curTag <- Random().Next(0, N)
                        stringTag <- stringTag + "#Tag" + (curTag |> string)
            if numMen >= 1 then
                curMen <- Random().Next(0, N)
                stringMen <- "user" + (curMen |> string)
                if numMen > 1 then
                    for k = 1 to numMen do
                        curMen <- Random().Next(0, N)
                        stringMen <- stringMen + "@user" + (curMen |> string)
            tweetContent <- "(" + (i|>string) + "th user " + (j|>string) + "th tweets)"
            msgToServer <- "send,"+user1_id+","+tweetContent+",,,"+stringTag+","+stringMen
            let resp = RemoteServer <! msgToServer
            printfn "[Post]: %i th user post %i th tweets!" i j
            printfn "[Response from Server]: %s" (resp |> string)

    // Users retweets
    for i = 0 to N - 1 do
        user1_id <- "user" + (i|>string)

    




    0 // return an integer exit code