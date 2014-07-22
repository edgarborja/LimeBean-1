﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace LimeBean.Tests {

    [TestFixture]
    public class IntegrationTests {

        [Test]
        public void ImplicitTransactionsOnStoreAndTrash() {
            using(var conn = new SQLiteConnection("data source=:memory:")) {
                conn.Open();

                IDatabaseAccess db = new DatabaseAccess(conn);
                IStorage storage = new SQLiteStorage(db);
                IBeanCrud crud = new BeanCrud(storage, db);               

                (storage as DatabaseStorage).EnterFluidMode();

                var bean = crud.Dispense<ThrowingBean>();
                bean["foo"] = "ok";
                var id = crud.Store(bean);

                bean.Throw = true;
                bean["foo"] = "fail";

                try { crud.Store(bean); } catch { }
                Assert.AreEqual("ok", db.Cell<string>(true, "select foo from test where " + Bean.ID_PROP_NAME + " = ?", id));

                try { crud.Trash(bean); } catch { }
                Assert.IsTrue(db.Cell<int>(true, "select count(*) from test") > 0);
            }
        }

        class ThrowingBean : Bean {
            public bool Throw;

            public ThrowingBean()
                : base("test") {
            }

            protected internal override void AfterStore() {
                if(Throw)
                    throw new Exception();
            }

            protected internal override void AfterTrash() {
                if(Throw)
                    throw new Exception();
            }
        }

    }

}