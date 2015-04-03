namespace http2

open System.Collections.Generic

module hpack =
    type HeaderRepresentation = 
        | Literal of byte array
        | Index of int

    type EncodedHeaderBlock = HeaderRepresentation list

    type StaticTableEntry = {
        Name: string;
        Value: string option
    }

                     

    let staticHeaderTable =
        [
            1, {Name = ":authority"; Value = None};
            2, {Name = ":method"; Value = Some "GET"};
            3, {Name = ":method"; Value = Some "POST"};
            4, {Name = ":path"; Value = Some "/"};
            5, {Name = ":path"; Value = Some "/index.html"};
            6, {Name = ":scheme"; Value = Some "http"};
            7, {Name = ":scheme"; Value = Some "https"};
            8, {Name = ":status"; Value = Some "200"};
            9, {Name = ":status"; Value = Some "204"};
            10, {Name = ":status"; Value = Some "206"};
            11, {Name = ":status"; Value = Some "304"};                                
            12, {Name = ":status"; Value = Some "400"};
            13, {Name = ":status"; Value = Some "404"};                                 
            14, {Name = ":status"; Value = Some "500"};                                 
            15, {Name = "accept-charset"; Value = None};                   
            16, {Name = "accept-encoding"; Value = None};                  
            17, {Name = "accept-language"; Value = None};                  
            18, {Name = "accept-ranges"; Value = None};                    
            19, {Name = "accept"; Value = None};                           
            20, {Name = "access-control-allow-origin"; Value = None};      
            21, {Name = "age"; Value = None};                              
            22, {Name = "allow"; Value = None};                            
            23, {Name = "authorization"; Value = None};                    
            24, {Name = "cache-control"; Value = None};                    
            25, {Name = "content-disposition"; Value = None};              
            26, {Name = "content-encoding"; Value = None};                 
            27, {Name = "content-language"; Value = None};                 
            28, {Name = "content-length"; Value = None};                   
            29, {Name = "content-location"; Value = None};                 
            30, {Name = "content-range"; Value = None};                    
            31, {Name = "content-type"; Value = None};                      
            32, {Name = "cookie"; Value = None};                           
            33, {Name = "date"; Value = None};                             
            34, {Name = "etag"; Value = None};                              
            35, {Name = "expect"; Value = None};                           
            36, {Name = "expires"; Value = None};                          
            37, {Name = "from"; Value = None};                             
            38, {Name = "host"; Value = None};                             
            39, {Name = "if-match"; Value = None};                         
            40, {Name = "if-modified-since"; Value = None};                
            41, {Name = "if-none-match"; Value = None};                    
            42, {Name = "if-range"; Value = None};                         
            43, {Name = "if-unmodified-since"; Value = None};              
            44, {Name = "last-modified"; Value = None};                    
            45, {Name = "link"; Value = None};                             
            46, {Name = "location"; Value = None};                         
            47, {Name = "max-forwards"; Value = None};                     
            48, {Name = "proxy-authenticate"; Value = None};               
            49, {Name = "proxy-authorization"; Value = None};              
            50, {Name = "range"; Value = None};                            
            51, {Name = "referer"; Value = None};                          
            52, {Name = "refresh"; Value = None};                          
            53, {Name = "retry-after"; Value = None};                      
            54, {Name = "server"; Value = None};                           
            55, {Name = "set-cookie"; Value = None};                       
            56, {Name = "strict-transport-security"; Value = None};        
            57, {Name = "transfer-encoding"; Value = None};                
            58, {Name = "user-agent"; Value = None};                       
            59, {Name = "vary"; Value = None};                             
            60, {Name = "via"; Value = None};                              
            61, {Name = "www-authenticate"; Value = None};
        ]
        |> Map.ofList
        