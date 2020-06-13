using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

class Window : Form {
    Graph graph;
    bool can_click = false; // Can click submit button
    bool finished = false; // Draw answers when true
    bool down = false; // Mouse down when true
    // bool hover = false;
    bool move = false;
    bool next = false;
    Dictionary<int, Point> vertexPositions = new Dictionary<int, Point>();
    public Window(Graph graph) {
        Text = "Investment Game";
        this.graph = graph;
        Point point2 = new Point();
        point2.X = 60 + 48;
        point2.Y = 300 - (int)((graph.drawn[2016] / graph.max_price) * 210);
        vertexPositions.Add(2016, point2);
        Point point1 = new Point();
        point1.X = 60;
        point1.Y = 300;
        vertexPositions.Add(2015, point1);
    }

    // Given an x-coordinates returns corresponding year
    public int year(int x) {
        if (108 < x && 108 + 96 >= x)
            return 2017;
        else if (108 + 96 < x && 108 + 96*2 >= x)
            return 2018;
        else if (108 + 96*2 < x && 108 + 96*3 >= x)
            return 2019;
        else if (108 + 96*3 < x && 108 + 96*4 >= x)
            return 2020;
        else if (108 + 96*4 < x && 108 + 96*5 >= x)
            return 2021;
        else
            return -1;
    }

    // Given a y-coordinate returns corresponding price
    public double price(int y) {
        return ((300 - y)/210) * graph.max_price;
    }

    protected override void OnPaint(PaintEventArgs args) {
        Graphics g = args.Graphics;


        Pen pen = new Pen(Color.FromArgb(255, 0, 0, 0), 2);
        Pen thinPen = new Pen(Color.FromArgb(255, 0, 0, 0), 1);
        thinPen.DashCap = System.Drawing.Drawing2D.DashCap.Round;
        thinPen.Color = Color.LightGray;
        thinPen.DashPattern = new float[]{4.0F, 4.0F, 4.0F, 4.0F};

        Font drawFont = new Font("Arial", 8);
        SolidBrush blackBrush = new SolidBrush(Color.Black);

        // Axis
        g.DrawLine(pen, 60, 300, 540, 300); // X-axis
        int yea = 2016;
        for (int i = 60 + 48; i < 540; i += 96) { // Horizontal labels
            g.DrawLine(pen, i, 295, i, 305);
            g.DrawString(yea.ToString(), drawFont, blackBrush, i - 15, 310);
            yea += 1;
            g.DrawLine(thinPen, i, 300, i, 80); // Vertical dashed lines
        }
        g.DrawLine(pen, 60, 300, 60, 80); // Y-axis
        g.DrawLine(thinPen, 540, 300, 540, 80);
        g.DrawString("June", drawFont, blackBrush, 60 - 15, 310);
        g.DrawString("June", drawFont, blackBrush, 540 - 15, 310);

        // Stock text
        g.DrawString(graph.name, new Font("Serif", 12), blackBrush, 400, 20);
        g.DrawString(graph.ticker, new Font("Serif", 9), blackBrush, 400, 40);

        // Button
        if (can_click) {
            g.FillRectangle(new SolidBrush(Color.IndianRed), 220, 340, 180, 40);
        } else {
            g.FillRectangle(new SolidBrush(Color.LightGray), 220, 345, 170, 40);
        }
        if (finished)
            g.DrawString("Try another".ToUpper(), new Font("Serif", 10), new SolidBrush(Color.White), 255, 355);
        else
            g.DrawString("Show me how I did".ToUpper(), new Font("Serif", 10), new SolidBrush(Color.White), 240, 355);   

        // Horizontal dashed lines
        int x = 0;
        for (int i = 300; i >= 90; i -= (300-90)/(graph.max_price / graph.division)) {
            if (i != 300) {
                g.DrawLine(thinPen, 60, i, 540, i);
            }
            if (x != 0)
                g.DrawString((graph.division * x).ToString(), new Font("Serif", 9), blackBrush, 32, i - 8);
            x += 1;
        }

        // Draw existing
        Pen redPen = new Pen(Color.Red, 2);
        // g.DrawLine(redPen, 60, 300, 60 + 48, 300 - (int)((graph.drawn[2016] / graph.max_price) * 210));
        int ye = 2015;
        while (vertexPositions.ContainsKey(ye + 1)) {
            g.DrawLine(redPen, vertexPositions[ye], vertexPositions[ye + 1]);
            ye += 1;
        }

        // Drawing winning as well
        Pen greenPen = new Pen(Color.Green, 2); 
        if (finished) {
            for (int i = 2016; i <= 2020; ++i) {
                Point pt1 = new Point();
                pt1.X = 108 + (i - 2016)*96;
                pt1.Y = 300 -  (int)((graph.actual_vertices[i]/graph.max_price)*210);
                Point pt2 = new Point();
                pt2.X = 108 + (i + 1 - 2016)*96;
                if (i + 1 == 2021)
                    pt2.X -= 48;
                pt2.Y = 300 - (int)((graph.actual_vertices[i + 1]/graph.max_price)*210);
                g.DrawLine(greenPen, pt1, pt2);
            }
        }

        // Hover
        // if (hover) {
        //     Point p = new Point();
        //     p.X = Cursor.Position.X - 10;
        //     p.Y = Cursor.Position.Y - 10;
        //     g.DrawString(graph.actual_vertices[year(Cursor.Position.X - 10)].ToString(), new Font("Serif", 9), blackBrush, p);
        // }

        // foreach (int y in vertexPositions.Keys) {
        //     Point p = vertexPositions[y];
        //     if (Math.Abs(Cursor.Position.X  - p.X) <= 15 && Math.Abs(Cursor.Position.Y - p.Y) <= 15) {
        //         // Point po = new Point();
        //         // p.X = Cursor.Position.X - 10;
        //         // p.Y = Cursor.Position.Y - 10;
        //         g.DrawString(graph.actual_vertices[year(Cursor.Position.X - 15)].ToString(), new Font("Serif", 9), blackBrush, p);
        //     }
                
        // }
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        if (down & !finished) {
            foreach (int yyear in vertexPositions.Keys) {
                if (yyear == 2016)
                    continue;
                if (Math.Abs(e.X - vertexPositions[yyear].X) <= 15 && Math.Abs(e.Y - vertexPositions[yyear].Y) <= 15) {
                    if (e.Y >= 80 && e.Y <= 300) {
                        vertexPositions[yyear] = new Point(vertexPositions[yyear].X, e.Y);
                        // Invalidate();
                        move = true;
                        return;
                    }
                }
            }
        }
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        down = true;
        if (e.Y <= 300 && e.Y >= 90) {
            if (108 < e.X && 108 + 96 >= e.X && vertexPositions.ContainsKey(year(e.X) - 1) 
            && vertexPositions.ContainsKey(year(e.X)) == false) {
                Point p = new Point();
                p.X = 108 + 96;
                p.Y = e.Y;
                vertexPositions.Add(year(e.X), p);
                graph.addPoint(year(e.X), price(e.Y));
                Invalidate();
            }
            else if (108 + 96*1 < e.X && 108 + 96*2 >= e.X && vertexPositions.ContainsKey(year(e.X) - 1)
            && vertexPositions.ContainsKey(year(e.X)) == false) {
                Point p = new Point();
                p.X = 108 + 96*2;
                p.Y = e.Y;
                vertexPositions.Add(year(e.X), p);
                graph.addPoint(year(e.X), price(e.Y));
                Invalidate();
            }
            else if (108 + 96*2 < e.X && 108 + 96*3 >= e.X && vertexPositions.ContainsKey(year(e.X) - 1)
            && vertexPositions.ContainsKey(year(e.X)) == false) {
                Point p = new Point();
                p.X = 108 + 96*3;
                p.Y = e.Y;
                vertexPositions.Add(year(e.X), p);
                graph.addPoint(year(e.X), price(e.Y));
                Invalidate();
            }
            else if (108 + 96*3 < e.X && 108 + 96*4 >= e.X && vertexPositions.ContainsKey(year(e.X) - 1)
            && vertexPositions.ContainsKey(year(e.X)) == false) {
                Point p = new Point();
                p.X = 108 + 96*4;
                p.Y = e.Y;
                vertexPositions.Add(year(e.X), p);
                graph.addPoint(year(e.X), price(e.Y));
                Invalidate();
            }
            else if (108 + 96*4 < e.X && 108 + 96*5 >= e.X && vertexPositions.ContainsKey(year(e.X) - 1)
            && vertexPositions.ContainsKey(year(e.X)) == false) {
                Point p = new Point();
                p.X = 108 + 96*5 - 48;
                p.Y = e.Y;
                vertexPositions.Add(year(e.X), p);
                graph.addPoint(year(e.X), price(e.Y));
                can_click = true; // Can click is now true
                Invalidate();
            }
        }
        if (can_click && !finished)
            if (e.X >= 220 && e.X <= 220 + 170 && e.Y <= 385 && e.Y >= 345 ) {
                finished = true;
                Invalidate();
            }
        if (can_click && finished) 
            if (next) {
                GetNextReady();
            }
            if (e.X >= 220 && e.X <= 220 + 170 && e.Y <= 385 && e.Y >= 345 ) {
                // Next
                next = true;
            }
    }

    public void GetNextReady() {
        can_click = false; // Can click submit button
        finished = false; // Draw answers when true
        down = false; // Mouse down when true
        move = false;
        next = false;
        vertexPositions = new Dictionary<int, Point>();
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        down = false;
        if (move) {
            move = false;
            Invalidate();
        }
    }

    // Hover over vertex reveals price
    // protected override void OnMouseHover(EventArgs e) {
    //     foreach (int ye in vertexPositions.Keys) {
    //         Point p = vertexPositions[ye];
    //         if (Math.Abs(Cursor.Position.X  - p.X) <= 15 && Math.Abs(Cursor.Position.Y - p.Y) <= 15)
    //             hover = true;
    //         else 
    //             hover = false;
    //     }
    //     Invalidate();
    // }

    [STAThread]
    static void Main() {
        Graph graph = new Graph();
        Form form = new Window(graph);
        form.Height = 450;
        form.Width = 600;
        form.BackColor = Color.White;
        form.KeyPreview = true;
        graph.changed += form.Invalidate;
        Application.Run(form);
    }   
}

delegate void Notify();

class Graph {
    public Notify changed;

    class Stock {
        string ticker;
        string name;
        int max_price;
        int division;
        Dictionary<int, double> actual_vertices = new Dictionary<int, double>();
    }

    Dictionary<string, string> stock = new Dictionary<string, string>();

    public string ticker {
        get {
            return stock["ticker"];
        }
    }

    public string name {
        get {
            return stock["name"];
        }
    }

    public int max_price {
        get {
            return 400;
        }
        set {
            max_price = value;
        }
    }

    public int division {
        get {
            return 100;
        }
        set {
            division = value;
        }
    }

    public Dictionary<int, double> drawn {
        get {
            return drawn_vertices;
        }
    }

    public Dictionary<int, double> actual_vertices = new Dictionary<int, double>();
    Dictionary<int, double> drawn_vertices = new Dictionary<int, double>();
    public Graph() {
        actual_vertices.Add(2015, 127.17);
        actual_vertices.Add(2016, 96.96);
        actual_vertices.Add(2017, 115.82);
        actual_vertices.Add(2018, 169.23);
        actual_vertices.Add(2019, 156.23);
        actual_vertices.Add(2020, 289.80);
        actual_vertices.Add(2021, 331.50);

        drawn_vertices.Add(2015, 127.17);
        drawn_vertices.Add(2016, 96.96);

        stock.Add("ticker", "NASDAQ: AAPL");
        stock.Add("name", "Apple Inc.");

        // Stock stock = new Stock();
    }

    public void addPoint(int year, double price) {
        drawn_vertices.Add(year, price);
        changed?.Invoke();
    }

    public void changePrice(int year, double price) {
        drawn_vertices[year] = price;
        changed?.Invoke();
    }

}